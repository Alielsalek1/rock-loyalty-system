using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoyaltyApi.Data;
using LoyaltyApi.Models;
using LoyaltyApi.Utilities;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyApi.Repositories
{
    public class VoucherFrontendRepository(FrontendDbContext dbContext,
    VoucherUtility voucherUtility,
    ILogger<VoucherFrontendRepository> logger) : IVoucherRepository
    {
        public async Task<Voucher> CreateVoucherAsync(Voucher voucher, Restaurant restaurant)
        {
            voucher.LongCode = Guid.NewGuid().ToString();
            voucher.ShortCode = voucherUtility.ShortenVoucherCode(voucher.LongCode);
            await dbContext.Vouchers.AddAsync(voucher);
            await dbContext.SaveChangesAsync();
            return voucher;
        }

        public async Task<PagedVouchersResponse> GetUserVouchersAsync(int customerId, int restaurantId, int pageNumber, int pageSize)
        {
            logger.LogInformation("Getting vouchers for customer {CustomerId} and restaurant {RestaurantId}",
                customerId, restaurantId);
            var query = dbContext.Vouchers
                .Where(v => v.CustomerId == customerId && v.RestaurantId == restaurantId)
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