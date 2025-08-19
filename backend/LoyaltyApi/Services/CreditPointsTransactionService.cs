using System.Numerics;
using System.Security.Claims;
using LoyaltyApi.Data;
using LoyaltyApi.Exceptions;
using LoyaltyApi.Models;
using LoyaltyApi.Repositories;
using LoyaltyApi.RequestModels;
using LoyaltyApi.Utilities;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LoyaltyApi.Services;

public class CreditPointsTransactionService(
    ICreditPointsTransactionRepository transactionRepository,
    ICreditPointsTransactionDetailRepository transactionDetailRepository,
    IRestaurantRepository restaurantRepository,
    FrontendDbContext context,
    IHttpContextAccessor httpContext,
    CreditPointsUtility creditPointsUtility,
    IUserRepository userRepository,
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

    public async Task<PagedTransactionsResponse> GetAllTransactionsByCustomerAndRestaurantAsync(int? customerId, int? restaurantId, int pageNumber = 1, int pageSize = 10)
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
            await ExpirePointsAsync(restaurantId ?? throw new ArgumentException("restaurantId not found"), customerId ?? throw new ArgumentException("customerId not found"));
            return await transactionRepository.GetAllTransactionsByCustomerAndRestaurantAsync(customerId ?? customerIdJwt,
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
            TransactionDate = transactionRequest.TransactionDate ?? DateTime.Now,
        };
        await transactionRepository.AddTransactionAsync(transaction);
    }

    public async Task SpendPointsAsync(int customerId, int restaurantId, int points)
    {
        logger.LogInformation("Spending {Points} points for customer {CustomerId} at restaurant {RestaurantId}", points,
            customerId, restaurantId);
        var restaurant = await restaurantRepository.GetRestaurantById(restaurantId) ??
                         throw new ArgumentException("Invalid restaurant");

        await ExpirePointsAsync(restaurantId, customerId);

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

        await ExpirePointsAsync(restaurantId ?? throw new ArgumentException("restaurantId not found"), customerId ?? throw new ArgumentException("customerId not found"));

        return await transactionRepository.GetCustomerPointsAsync(customerId ?? customerIdJwt,
            restaurantId ?? restaurantIdJwt);
    }

    public async Task<int> ExpirePointsAsync(int restaurantId, int customerId)
    {
        logger.LogInformation("Expiring points points for customer {CustomerId} at restaurant {RestaurantId}",
            customerId, restaurantId);

        Restaurant? restaurant = await restaurantRepository.GetRestaurantById(restaurantId) ??
           throw new NullReferenceException("Restaurant not found");
        User customer = new()
        {
            Id = customerId,
            RestaurantId = restaurant.RestaurantId

        };
        User user = await userRepository.GetUserAsync(customer) ?? throw new NullReferenceException("User not found");
        await using var dbTransaction = await context.Database.BeginTransactionAsync(); // Start a database transaction

        try
        {
            logger.LogInformation("Starting point expiration process");

            //TODO::Useless shit
            // Load all restaurant data into memory (ID and CreditPointsLifeTime)
            // logger.LogInformation("Fetching all restaurants for point expiration");
            // var restaurants = await restaurantRepository.GetAllRestaurantsAsync();
            // logger.LogInformation("Fetched {restaurants} restaurants for point expiration", restaurants);
            // var restaurantMap = restaurants.ToDictionary(
            //     r => r.RestaurantId, restaurant => restaurant);
            // var currentDateTime = DateTime.Now;
            // Fetch all transactions that have expired based on the restaurant's lifetime

            var currentDateTime = DateTime.Now.AddHours(23).AddMinutes(53); // for testing
            var expiredTransactions =
                await transactionRepository.GetExpiredTransactionsByCustomerAndRestaurantAsync(restaurant, user.Id, currentDateTime);

            if (!expiredTransactions.Any())
            {
                logger.LogInformation("No expired transactions found for customer {CustomerId} at restaurant {RestaurantId}",
                    user.Id, restaurant.RestaurantId);
                return 0;
            }

            // Create lists to hold new expiration transactions (no need for transaction details anymore)
            var expiringTanscations = new List<CreditPointsTransaction>();

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
                    // Create an expire transaction (similar to spend transaction)
                    var expireTransaction = new CreditPointsTransaction
                    {
                        CustomerId = transaction.CustomerId,
                        RestaurantId = transaction.RestaurantId,
                        Points = -remainingPoints,
                        TransactionType = TransactionType.Expire,
                        TransactionDate = currentDateTime,
                        TransactionValue = remainingPoints * restaurant.CreditPointsSellingRate,
                    };
                    expiringTanscations.Add(expireTransaction);

                    transaction.IsExpired = true; // Mark the original `earn` transaction as expired
                    await transactionRepository.UpdateTransactionAsync(transaction);
                }
            }

            // Add new expiration transactions to database
            await transactionRepository.AddTransactionsAsync(expiringTanscations);

            // Commit the changes
            await context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return expiringTanscations.Count;
        }
        catch
        {
            await dbTransaction.RollbackAsync(); // Rollback transaction on error
            throw new ExpirePointsFailedException("Failed to expire points");
        }
    }
}