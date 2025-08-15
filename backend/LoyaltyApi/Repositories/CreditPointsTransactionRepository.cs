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
        return await dbContext.CreditPointsTransactions
            .Include(t => t.CreditPointsTransactionDetails)
            .FirstOrDefaultAsync(t => t.TransactionId == transactionId);
    }

    public async Task<CreditPointsTransaction?> GetTransactionByReceiptIdAsync(long receiptId)
    {
        logger.LogInformation("Getting transaction for receipt {ReceiptId}", receiptId);
        return await dbContext.CreditPointsTransactions
            .Include(t => t.CreditPointsTransactionDetails)
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

    public async Task<PagedTransactionsResponse> GetTransactionsByCustomerAndRestaurantAsync(int customerId,
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

    public async Task<IEnumerable<CreditPointsTransaction>> GetExpiredTransactionsAsync(
        Dictionary<int, Restaurant> restaurantMap, DateTime currentDate)
    {
        // Use SQL query to get all transactions that have expired based on the restaurant's lifetime
        var query = from transaction in dbContext.CreditPointsTransactions
            join restaurant in dbContext.Restaurants
                on transaction.RestaurantId equals restaurant.RestaurantId
            where
                transaction.TransactionDate < currentDate.AddDays(-restaurant.CreditPointsLifeTime) &&
                transaction.TransactionType == TransactionType.Earn &&
                transaction.IsExpired == false &&
                transaction.Points > 0
            select transaction;
        logger.LogInformation("Getting expired transactions");
        return await query.ToListAsync();
    }
}