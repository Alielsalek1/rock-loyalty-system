using System.Numerics;
using System.Security.Claims;
using LoyaltyApi.Data;
using LoyaltyApi.Exceptions;
using LoyaltyApi.Models;
using LoyaltyApi.Repositories;
using LoyaltyApi.RequestModels;
using LoyaltyApi.Utilities;

namespace LoyaltyApi.Services;

public class CreditPointsTransactionService(
    ICreditPointsTransactionRepository transactionRepository,
    ICreditPointsTransactionDetailRepository transactionDetailRepository,
    IRestaurantRepository restaurantRepository,
    RockDbContext context,
    IHttpContextAccessor httpContext,
    CreditPointsUtility creditPointsUtility,
    ILogger<CreditPointsTransactionService> logger) : ICreditPointsTransactionService
{
    public async Task<CreditPointsTransaction?> GetTransactionByIdAsync(int transactionId)
    {
        logger.LogInformation("Getting transaction {TransactionId}", transactionId);
        try
        {
            return await transactionRepository.GetTransactionByIdAsync(transactionId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving transaction {TransactionId}", transactionId);
            throw new Exception("Error retrieving transaction");
        }
    }

    public async Task<CreditPointsTransaction?> GetTransactionByReceiptIdAsync(long receiptId)
    {
        logger.LogInformation("Getting transaction for receipt {ReceiptId}", receiptId);
        try
        {
            return await transactionRepository.GetTransactionByReceiptIdAsync(receiptId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving transaction for receipt {ReceiptId}", receiptId);
            throw new Exception("Error retrieving transaction");
        }
    }

    public async Task<PagedTransactionsResponse> GetTransactionsByCustomerAndRestaurantAsync
        (int? customerId, int? restaurantId, int pageNumber = 1, int pageSize = 10)
    {
        logger.LogInformation("Getting transactions for customer {CustomerId} and restaurant {RestaurantId}",
            customerId, restaurantId);
        int customerIdJwt = customerId ??
                            int.Parse(httpContext.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                                      throw new ArgumentException("customerId not found"));
        logger.LogTrace("customerIdJwt: {customerIdJwt}", customerIdJwt);
        int restaurantIdJwt = restaurantId ??
                              int.Parse(httpContext.HttpContext?.User?.FindFirst("restaurantId")?.Value ??
                                        throw new ArgumentException("restaurantId not found"));
        logger.LogTrace("restaurantIdJwt: {restaurantIdJwt}", restaurantIdJwt);

        try
        {
            return await transactionRepository.GetTransactionsByCustomerAndRestaurantAsync(customerId ?? customerIdJwt,
                restaurantId ?? restaurantIdJwt, pageNumber, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving transactions for customer {CustomerId} and restaurant {RestaurantId}",
                customerId, restaurantId);
            throw new Exception("Error retrieving transactions");
        }
    }

    public async Task AddTransactionAsync(CreateTransactionRequest transactionRequest)
    {
        logger.LogInformation("Adding transaction for customer {CustomerId} and restaurant {RestaurantId}",
            transactionRequest.CustomerId, transactionRequest.RestaurantId);

        var restaurant = await restaurantRepository.GetRestaurantById(transactionRequest.RestaurantId) ??
                         throw new ArgumentException("Invalid restaurant");


        var pointsEarned = creditPointsUtility.CalculateCreditPoints(transactionRequest.Amount,
                restaurant.CreditPointsBuyingRate);

        logger.LogInformation("Points earned: {PointsEarned} from amount {Amount}", pointsEarned, transactionRequest.Amount);

        if (pointsEarned == 0) throw new MinimumTransactionAmountNotReachedException("Point used too low");

        var transaction = new CreditPointsTransaction
        {
            CustomerId = transactionRequest.CustomerId,
            RestaurantId = transactionRequest.RestaurantId,
            ReceiptId = transactionRequest.ReceiptId,
            TransactionType = TransactionType.Earn,
            Points = pointsEarned,
            TransactionValue = transactionRequest.Amount,
            TransactionDate = transactionRequest.TransactionDate ?? DateTime.Now
        };
        await transactionRepository.AddTransactionAsync(transaction);
    }

    /// <summary>
    /// Spends the specified number of credit points for a customer at a restaurant.
    /// 
    /// This function starts a database transaction, retrieves the customer's available transactions, 
    /// checks if there are enough points to spend, creates a new spend transaction, and distributes 
    /// the points across available transactions.
    /// 
    /// Parameters:
    ///   customerId (int): The ID of the customer spending the points.
    ///   restaurantId (int): The ID of the restaurant where the points are being spent.
    ///   points (int): The number of points to spend.
    /// 
    /// Returns:
    ///   Task: A task representing the asynchronous operation.
    /// </summary>
    public async Task SpendPointsAsync(int customerId, int restaurantId, int points)
    {
        logger.LogInformation("Spending {Points} points for customer {CustomerId} at restaurant {RestaurantId}", points,
            customerId, restaurantId);
        var restaurant = await restaurantRepository.GetRestaurantById(restaurantId) ??
                         throw new ArgumentException("Invalid restaurant");

        await using var dbTransaction = await context.Database.BeginTransactionAsync(); // Start a database transaction

        try
        {
            // Retrieve customer transactions and spend the points
            var transactions =
                await transactionRepository.GetAllTransactionsByCustomerAndRestaurantAsync(customerId, restaurantId);

            var remainingPoints = points;

            // Check if there are enough points before proceeding
            var totalAvailablePoints = transactions.Sum(t => t.Points);
            
            if (totalAvailablePoints < points)
            {
                throw new PointsNotEnoughException("Not enough points");
            }

            // Create a new spend transaction
            var spendTransaction = new CreditPointsTransaction
            {
                CustomerId = customerId,
                RestaurantId = restaurantId,
                Points = -points,
                TransactionType = TransactionType.Spend,
                TransactionDate = DateTime.Now,
                TransactionValue = points / restaurant.CreditPointsBuyingRate
            };

            await context.CreditPointsTransactions.AddAsync(spendTransaction);
            await context.SaveChangesAsync();

            // Distribute the points to spend across available transactions
            foreach (var transaction in transactions
                         .Where(transaction =>
                            transaction.TransactionDate > DateTime.Now.AddDays(-restaurant.CreditPointsLifeTime) &&
                            transaction.TransactionType == TransactionType.Earn)
                         .OrderBy(t => t.TransactionDate))
            {
                if (remainingPoints <= 0) break;

                var spentPoints =
                    await transactionDetailRepository.GetTotalPointsSpentForEarnTransaction(transaction.TransactionId);

                var availablePoints = transaction.Points - spentPoints;

                if (availablePoints <= 0) continue;

                var pointsToUse = Math.Min(availablePoints, remainingPoints);
                remainingPoints -= pointsToUse;

                var transactionDetail = new CreditPointsTransactionDetail
                {
                    TransactionId = spendTransaction.TransactionId,
                    EarnTransactionId = transaction.TransactionId,
                    PointsUsed = pointsToUse,
                };

                await context.CreditPointsTransactionsDetails.AddAsync(transactionDetail);
            }

            await context.SaveChangesAsync();
            await dbTransaction.CommitAsync();
        }
        catch
        {
            await dbTransaction.RollbackAsync(); // Rollback if any error occurs
            throw;
        }
    }

    public async Task<int> GetCustomerPointsAsync(int? customerId, int? restaurantId)
    {
        logger.LogInformation("Getting customer points for customer {CustomerId} and restaurant {RestaurantId}",
            customerId, restaurantId);
        int customerIdJwt = customerId ??
                            int.Parse(httpContext.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                                      throw new ArgumentException("customerId not found"));
        logger.LogTrace("customerIdJwt: {customerIdJwt}", customerIdJwt);
        int restaurantIdJwt = restaurantId ??
                              int.Parse(httpContext.HttpContext?.User?.FindFirst("restaurantId")?.Value ??
                                        throw new ArgumentException("restaurantId not found"));
        logger.LogTrace("restaurantIdJwt: {restaurantIdJwt}", restaurantIdJwt);

        return await transactionRepository.GetCustomerPointsAsync(customerId ?? customerIdJwt,
            restaurantId ?? restaurantIdJwt);
    }

    public async Task<int> ExpirePointsAsync()
    {
        logger.LogInformation("Expiring points");
        await using var dbTransaction = await context.Database.BeginTransactionAsync(); // Start a database transaction

        try
        {
            // Load all restaurant data into memory (ID and CreditPointsLifeTime)
            logger.LogInformation("Fetching all restaurants for point expiration");
            var restaurants = await restaurantRepository.GetAllRestaurantsAsync();
            logger.LogInformation("Fetched {restaurants} restaurants for point expiration", restaurants);
            var restaurantMap = restaurants.ToDictionary(
                r => r.RestaurantId, restaurant => restaurant);
            var currentDateTime = DateTime.Now;
            // Fetch all transactions that have expired based on the restaurant's lifetime
            var expiredTransactions =
                await transactionRepository.GetExpiredTransactionsAsync(restaurantMap, currentDateTime);

            if (!expiredTransactions.Any())
            {
                return 0; // No transactions to expire
            }

            // Create lists to hold new expiration transactions and transaction data
            var expirationTransactions = new List<CreditPointsTransaction>();
            var transactionDetailsData = new List<(int EarnTransactionId, int PointsUsed)>();

            // Process the expired transactions
            foreach (var transaction in expiredTransactions)
            {
                // Fetch total points spent from this earn transaction
                var pointsSpent =
                    await transactionDetailRepository.GetTotalPointsSpentForEarnTransaction(transaction.TransactionId);

                // Calculate remaining points that can be expired
                var remainingPoints = transaction.Points - pointsSpent;

                if (remainingPoints > 0)
                {
                    // Create an expire transaction
                    var expireTransaction = new CreditPointsTransaction
                    {
                        CustomerId = transaction.CustomerId,
                        RestaurantId = transaction.RestaurantId,
                        Points = -remainingPoints,
                        TransactionType = TransactionType.Expire,
                        TransactionDate = currentDateTime,
                        TransactionValue = remainingPoints * restaurantMap[transaction.RestaurantId].CreditPointsSellingRate
                    };
                    expirationTransactions.Add(expireTransaction);

                    // Store data for creating transaction details later
                    transactionDetailsData.Add((transaction.TransactionId, remainingPoints));

                    transaction.IsExpired = true; // Mark the original `earn` transaction as expired
                    await transactionRepository.UpdateTransactionAsync(transaction);
                }
            }

            // First, add new expiration transactions to get their IDs
            await transactionRepository.AddTransactionsAsync(expirationTransactions);

            // Now create transaction details with the correct TransactionIds
            var expirationDetails = new List<CreditPointsTransactionDetail>();
            foreach (var (transaction, detailData) in expirationTransactions.Zip(transactionDetailsData))
            {
                var expireDetail = new CreditPointsTransactionDetail
                {
                    TransactionId = transaction.TransactionId, // Now has the correct generated ID
                    EarnTransactionId = detailData.EarnTransactionId,
                    PointsUsed = detailData.PointsUsed
                };
                expirationDetails.Add(expireDetail);
            }

            // Add transaction details to the database
            await transactionDetailRepository.AddTransactionsDetailsAsync(expirationDetails);

            // Commit the changes
            await context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return expirationTransactions.Count;
        }
        catch
        {
            await dbTransaction.RollbackAsync(); // Rollback transaction on error
            throw;
        }
    }
}