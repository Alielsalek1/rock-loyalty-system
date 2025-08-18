using System.Numerics;
using LoyaltyApi.Models;
using LoyaltyApi.RequestModels;

namespace LoyaltyApi.Services
{
    public interface ICreditPointsTransactionService
    {
        public Task<CreditPointsTransaction?> GetTransactionByIdAsync(int transactionId);

        public Task<CreditPointsTransaction?> GetTransactionByReceiptIdAsync(long receiptId);

        public Task<PagedTransactionsResponse> GetAllTransactionsByCustomerAndRestaurantAsync(int? customerId,
            int? restaurantId, int pageNumber, int pageSize);
        public Task<PagedTransactionsResponse> GetViableTransactionsByCustomerAndRestaurantAsync(int? customerId,
       int? restaurantId, int pageNumber, int pageSize);

        public Task AddTransactionAsync(CreateTransactionRequest transactionRequest);

        public Task SpendPointsAsync(int customerId, int restaurantId, int points);

        public Task<int> GetCustomerPointsAsync(int? customerId, int? restaurantId);

        public Task<int> ExpirePointsAsync(int restaurantId, int customerId);
    }
}