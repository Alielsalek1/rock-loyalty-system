using LoyaltyApi.Models;
using LoyaltyApi.RequestModels;

namespace LoyaltyApi.Services
{
    public interface IVoucherService
    {
        Task<PagedVouchersResponse> GetUserVouchersAsync(int customerId, int restaurantId, int pageNumber, int pageSize);

        Task<Voucher> GetVoucherAsync(string shortCode);

        Task<Voucher> CreateVoucherAsync(CreateVoucherRequest voucherRequest, int customerId, int restaurantId);
        Task<Voucher> SetIsUsedAsync(string shortCode);
    }
}