using System.Numerics;
using LoyaltyApi.Models;

namespace LoyaltyApi.Repositories;

public interface ICreditPointsTransactionRepository
{
    public Task<CreditPointsTransaction?> GetTransactionByIdAsync(int transactionId);

    public Task<CreditPointsTransaction?> GetTransactionByReceiptIdAsync(long receiptId);

    Task<IEnumerable<CreditPointsTransaction>> GetAllTransactionsByCustomerAndRestaurantAsync(int customerId, int restaurantId);

    public Task<PagedTransactionsResponse> GetAllTransactionsByCustomerAndRestaurantAsync(int customerId,
        int restaurantId, int pageNumber = 1, int pageSize = 10);

    public Task AddTransactionAsync(CreditPointsTransaction transaction);

    public Task AddTransactionsAsync(List<CreditPointsTransaction> transactions);

    public Task UpdateTransactionAsync(CreditPointsTransaction transaction);

    public Task DeleteTransactionAsync(int transactionId);

    public Task<int> GetCustomerPointsAsync(int customerId, int restaurantId);

    public Task<IEnumerable<CreditPointsTransaction>> GetExpiredTransactionsByCustomerAndRestaurantAsync(Restaurant restaurant, int customerId, DateTime currentDate);

    public Task<PagedTransactionsResponse> GetViableTransactionsByCustomerAndRestaurantAsync(int customerId,
    int restaurantId, int pageNumber = 1, int pageSize = 10);

    // New methods to replace transaction detail functionality
    public Task<int> GetTotalPointsSpentForEarnTransaction(int earnTransactionId);

}