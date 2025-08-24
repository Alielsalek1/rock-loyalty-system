using LoyaltyApi.Models;

namespace LoyaltyApi.Repositories
{
    public interface IVoucherRepository
    {
        Task<Voucher> CreateVoucherAsync(Voucher voucher, Restaurant restaurant);

        Task<Voucher?> GetVoucherAsync(string shortCode);
        Task<Voucher> UpdateVoucherAsync(Voucher voucher);

        Task<PagedVouchersResponse> GetUserVouchersAsync(int customerId, int restaurantId, int pageNumber, int pageSize);

    }
}