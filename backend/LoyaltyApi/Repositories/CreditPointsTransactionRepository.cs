using System.Numerics;
using LoyaltyApi.Data;
using LoyaltyApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyApi.Repositories;

public class CreditPointsTransactionRepository(
    FrontendDbContext dbContext,
    ILogger<CreditPointsTransactionRepository> logger) : ICreditPointsTransactionRepository
{
    public async Task<CreditPointsTransaction?> GetTransactionByIdAsync(int transactionId)
    {
        logger.LogInformation("Getting transaction {TransactionId}", transactionId);
        return await dbContext.CreditPointsTransactions.FirstOrDefaultAsync(t => t.TransactionId == transactionId);
    }

    public async Task<CreditPointsTransaction?> GetTransactionByReceiptIdAsync(long receiptId)
    {
        logger.LogInformation("Getting transaction for receipt {ReceiptId}", receiptId);
        return await dbContext.CreditPointsTransactions
            .FirstOrDefaultAsync(t => t.ReceiptId == receiptId);
    }

    public async Task<IEnumerable<CreditPointsTransaction>> GetAllTransactionsByCustomerAndRestaurantAsync(
        int customerId, int restaurantId)
    {
        logger.LogInformation("Getting transactions for customer {CustomerId} and restaurant {RestaurantId}",
            customerId, restaurantId);
        return await dbContext.CreditPointsTransactions
            .Where(t => t.RestaurantId == restaurantId && t.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<PagedTransactionsResponse> GetAllTransactionsByCustomerAndRestaurantAsync(int customerId,
        int restaurantId, int pageNumber, int pageSize)
    {
        logger.LogInformation("Getting transactions for customer {CustomerId} and restaurant {RestaurantId}",
            customerId, restaurantId);
        var query = dbContext.CreditPointsTransactions
            .Where(t => t.RestaurantId == restaurantId && t.CustomerId == customerId)
            .OrderByDescending(t => t.TransactionId)
            .AsQueryable();
        var totalCount = await query.CountAsync();
        var paginatedQuery = query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
        var transactions = await paginatedQuery.ToListAsync();

        var response = new PagedTransactionsResponse
        {
            Transactions = transactions,
            PaginationMetadata = new PaginationMetadata
            {
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                PageSize = pageSize,
                PageNumber = pageNumber
            }
        };

        return response;
    }

    public async Task AddTransactionAsync(CreditPointsTransaction transaction)
    {
        logger.LogInformation("Adding transaction {TransactionId}", transaction.TransactionId);
        await dbContext.CreditPointsTransactions.AddAsync(transaction);
        await dbContext.SaveChangesAsync();
    }

    public async Task AddTransactionsAsync(List<CreditPointsTransaction> transactions)
    {
        await dbContext.CreditPointsTransactions.AddRangeAsync(transactions);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Transactions created successfully");
    }

    public async Task UpdateTransactionAsync(CreditPointsTransaction transaction)
    {
        dbContext.CreditPointsTransactions.Update(transaction);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Transaction {TransactionId} updated successfully", transaction.TransactionId);
    }

    public async Task DeleteTransactionAsync(int transactionId)
    {
        var transaction = await GetTransactionByIdAsync(transactionId);
        if (transaction is not null)
        {
            dbContext.CreditPointsTransactions.Remove(transaction);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Transaction {TransactionId} deleted successfully", transaction.TransactionId);
        }
    }

    public async Task<int> GetCustomerPointsAsync(int customerId, int restaurantId)
    {
        logger.LogInformation("Getting total points for customer {CustomerId} and restaurant {RestaurantId}",
            customerId, restaurantId);
        return await dbContext.CreditPointsTransactions
            .Where(t => t.RestaurantId == restaurantId && t.CustomerId == customerId)
            .SumAsync(t => t.Points);
    }

    public async Task<IEnumerable<CreditPointsTransaction>> GetExpiredTransactionsByCustomerAndRestaurantAsync(
        Restaurant restaurant, int customerId, DateTime currentDate)
    {
        logger.LogInformation("Getting expired transactions for customer {CustomerId} and restaurant {RestaurantId}",
            customerId, restaurant.RestaurantId);

        var expirationDate = currentDate.AddDays(-restaurant.CreditPointsLifeTime);

        var expiredTransactions = await dbContext.CreditPointsTransactions
            .Where(t => t.RestaurantId == restaurant.RestaurantId
                     && t.CustomerId == customerId
                     && t.TransactionDate < expirationDate
                     && t.TransactionType == TransactionType.Earn
                     && !t.IsExpired
                     && t.Points > 0)
            .ToListAsync();

        return expiredTransactions;
    }

    public async Task<PagedTransactionsResponse> GetViableTransactionsByCustomerAndRestaurantAsync(int customerId, int restaurantId, int pageNumber = 1, int pageSize = 10)
    {
        logger.LogInformation("Getting Viable transactions for customer {CustomerId} and restaurant {RestaurantId}",
         customerId, restaurantId);
        var query = dbContext.CreditPointsTransactions
            .Where(t => t.RestaurantId == restaurantId && t.CustomerId == customerId && !t.IsExpired && t.Points > 0)
            .OrderByDescending(t => t.TransactionId)
            .AsQueryable();
        var totalCount = await query.CountAsync();
        var paginatedQuery = query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
        var transactions = await paginatedQuery.ToListAsync();

        var response = new PagedTransactionsResponse
        {
            Transactions = transactions,
            PaginationMetadata = new PaginationMetadata
            {
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                PageSize = pageSize,
                PageNumber = pageNumber
            }
        };

        return response;
    }


    public async Task<int> GetTotalPointsSpentForEarnTransaction(int earnTransactionId)
    {
        logger.LogInformation("Getting total points spent for earn transaction {EarnTransactionId}", earnTransactionId);

        var totalPointsSpent = await dbContext.CreditPointsTransactions
            .Where(t => t.EarnTransactionId == earnTransactionId &&
                       (t.TransactionType == TransactionType.Spend || t.TransactionType == TransactionType.Expire))
            .SumAsync(t => Math.Abs(t.Points));

        logger.LogInformation("Total points spent for earn transaction {EarnTransactionId}: {TotalPointsSpent}",
            earnTransactionId, totalPointsSpent);

        return totalPointsSpent;
    }
}