using LoyaltyApi.Data;
using LoyaltyApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyApi.Repositories;

public class CreditPointsTransactionDetailRepository(RockDbContext dbContext,
ILogger<CreditPointsTransactionDetailRepository> logger) : ICreditPointsTransactionDetailRepository
{
    public async Task<CreditPointsTransactionDetail?> GetTransactionDetailByIdAsync(int transactionDetailId)
    {
        logger.LogInformation("Getting transaction detail {TransactionDetailId}", transactionDetailId);
        return await dbContext.CreditPointsTransactionsDetails
            .FirstOrDefaultAsync(t => t.DetailId == transactionDetailId);
    }

    public async Task<CreditPointsTransactionDetail?> GetTransactionDetailByTransactionIdAsync(int transactionId)
    {
        logger.LogInformation("Getting transaction detail for transaction {TransactionId}", transactionId);
        return await dbContext.CreditPointsTransactionsDetails
            .FirstOrDefaultAsync(t => t.TransactionId == transactionId);
    }

    public async Task AddTransactionDetailAsync(CreditPointsTransactionDetail transactionDetail)
    {
        await dbContext.CreditPointsTransactionsDetails.AddAsync(transactionDetail);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Transaction detail {DetailId} created successfully", transactionDetail.DetailId);
    }

    public async Task AddTransactionsDetailsAsync(List<CreditPointsTransactionDetail> details)
    {
        await dbContext.CreditPointsTransactionsDetails.AddRangeAsync(details);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Transaction details created successfully");
    }

    public async Task UpdateTransactionDetailAsync(CreditPointsTransactionDetail transactionDetail)
    {
        dbContext.CreditPointsTransactionsDetails.Update(transactionDetail);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Transaction detail {DetailId} updated successfully", transactionDetail.DetailId);
    }

    public async Task DeleteTransactionDetailAsync(int transactionDetailId)
    {
        var transactionDetail = await GetTransactionDetailByIdAsync(transactionDetailId);

        if (transactionDetail is not null)
        {
            dbContext.CreditPointsTransactionsDetails.Remove(transactionDetail);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Transaction detail {DetailId} deleted successfully", transactionDetail.DetailId);
        }

    }
    public async Task<int> GetTotalPointsSpentForEarnTransaction(int earnTransactionId)
    {
        logger.LogInformation("Getting total points spent for earn transaction {EarnTransactionId}", earnTransactionId);

        var totalPointsSpent = await dbContext.CreditPointsTransactionsDetails
            .Where(detail => detail.EarnTransactionId == earnTransactionId)
            .SumAsync(detail => detail.PointsUsed);

        logger.LogInformation("Total points spent for earn transaction {EarnTransactionId}: {TotalPointsSpent}", earnTransactionId, totalPointsSpent);

        return totalPointsSpent;
    }

    public async Task<Dictionary<int, int>> GetTotalPointsSpentForMultipleEarnTransactions(IEnumerable<int> earnTransactionIds)
    {
        logger.LogInformation("Getting total points spent for multiple earn transactions");

        var result = await dbContext.CreditPointsTransactionsDetails
            .Where(detail => earnTransactionIds.Contains(detail.EarnTransactionId))
            .GroupBy(detail => detail.EarnTransactionId)
            .Select(group => new { EarnTransactionId = group.Key, TotalPointsSpent = group.Sum(detail => detail.PointsUsed) })
            .ToDictionaryAsync(x => x.EarnTransactionId, x => x.TotalPointsSpent);

        // Ensure all requested transaction IDs are in the result, even if they have 0 points spent
        foreach (var transactionId in earnTransactionIds)
        {
            if (!result.ContainsKey(transactionId))
            {
                result[transactionId] = 0;
            }
        }

        logger.LogInformation("Retrieved points spent data for {Count} transactions", result.Count);

        return result;
    }
}