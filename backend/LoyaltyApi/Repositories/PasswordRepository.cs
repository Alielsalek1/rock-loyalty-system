using LoyaltyApi.Data;
using LoyaltyApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyApi.Repositories
{
    public class PasswordRepository(FrontendDbContext dbContext,
    ILogger<PasswordRepository> logger) : IPasswordRepository
    {
        public async Task<Password> CreatePasswordAsync(Password password)
        {
            await dbContext.Passwords.AddAsync(password);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Password created successfully for customer {CustomerId} and restaurant {RestaurantId}", password.CustomerId, password.RestaurantId);
            return password;
        }



        public async Task<Password?> GetPasswordAsync(Password password)
        {
            logger.LogInformation("Getting password for customer {CustomerId} and restaurant {RestaurantId}", password.CustomerId, password.RestaurantId);
            return await dbContext.Passwords.FirstOrDefaultAsync(p => p.CustomerId == password.CustomerId && p.RestaurantId == password.RestaurantId);
        }

        public async Task<Password> UpdatePasswordAsync(Password password)
        {
            dbContext.Passwords.Update(password);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Password updated successfully for customer {CustomerId} and restaurant {RestaurantId}", password.CustomerId, password.RestaurantId);
            return password;
        }
    }
}