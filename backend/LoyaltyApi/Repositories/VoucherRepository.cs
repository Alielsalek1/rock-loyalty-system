using System.Data;
using System.Text.Json;
using LoyaltyApi.Config;
using LoyaltyApi.Data;
using LoyaltyApi.Models;
using LoyaltyApi.Utilities;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LoyaltyApi.Repositories
{
    public class VoucherRepository(
        RockDbContext dbContext,
        ApiUtility apiUtility,
        VoucherUtility voucherUtility,
        ILogger<VoucherRepository> logger) : IVoucherRepository
    {
        public async Task<Voucher> CreateVoucherAsync(Voucher voucher, Restaurant restaurant)
        {
            logger.LogInformation("Creating voucher {ShortCode} for customer {CustomerId} and restaurant {RestaurantId} with value {Value}",
            voucher.ShortCode,
            voucher.CustomerId,
            voucher.RestaurantId,
            voucher.Value);

            var apiKey = await apiUtility.GetApiKey(voucher.RestaurantId.ToString());

            string result = await apiUtility.GenerateVoucher(voucher, restaurant, apiKey);

            voucher.ShortCode = result;

            await dbContext.Vouchers.AddAsync(voucher);

            await dbContext.SaveChangesAsync();

            logger.LogInformation("Voucher {ShortCode} created successfully", voucher.ShortCode);

            return voucher;
        }

        public async Task<PagedVouchersResponse> GetUserVouchersAsync(int customerId, int restaurantId, int pageNumber, int pageSize)
        {
            logger.LogInformation("Getting vouchers for customer {CustomerId} and restaurant {RestaurantId}",
                customerId, restaurantId);
                
            var query = dbContext.Vouchers
                .Where(v => v.CustomerId == customerId && v.RestaurantId == restaurantId)
                .OrderByDescending(v => v.DateOfCreation)
                .AsQueryable();
            var totalCount = await query.CountAsync();
            var paginatedQuery = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);
            var vouchers = await paginatedQuery.ToListAsync();

            var response = new PagedVouchersResponse()
            {
                Vouchers = vouchers,
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

        public async Task<Voucher?> GetVoucherAsync(string shortCode)
        {
            logger.LogInformation("Getting voucher {ShortCode}",
                shortCode);
            return await dbContext.Vouchers.FindAsync(shortCode);
        }

        public async Task<Voucher> UpdateVoucherAsync(Voucher voucher)
        {
            logger.LogInformation("Updating voucher {ShortCode}",
                voucher.ShortCode);
            dbContext.Update(voucher);
            await dbContext.SaveChangesAsync();
            return voucher;
        }
    }
}