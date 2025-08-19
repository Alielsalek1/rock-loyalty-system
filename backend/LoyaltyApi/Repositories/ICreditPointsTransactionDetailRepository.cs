using LoyaltyApi.Models;

namespace LoyaltyApi.Repositories;

public interface ICreditPointsTransactionDetailRepository
{

    public Task<CreditPointsTransactionDetail?> GetTransactionDetailByIdAsync(int transactionDetailId);

    public Task<CreditPointsTransactionDetail?> GetTransactionDetailByTransactionIdAsync(int transactionId);

    public Task AddTransactionDetailAsync(CreditPointsTransactionDetail transactionDetail);

    public Task AddTransactionsDetailsAsync(List<CreditPointsTransactionDetail> details);

    public Task UpdateTransactionDetailAsync(CreditPointsTransactionDetail transactionDetail);

    public Task DeleteTransactionDetailAsync(int transactionDetailId);

    public Task<int> GetTotalPointsSpentForEarnTransaction(int earnTransactionId);

    public Task<Dictionary<int, int>> GetTotalPointsSpentForMultipleEarnTransactions(IEnumerable<int> earnTransactionIds);
}