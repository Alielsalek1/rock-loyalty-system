using LoyaltyApi.Data;
using LoyaltyApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyApi.Repositories
{
    public class UserFrontendRepository(
        FrontendDbContext dbContext,
        ILogger<UserRepository> logger) : IUserRepository
    {
        public async Task<User?> CreateUserAsync(User user)
        {
            logger.LogInformation("Creating user {id} for restaurant {RestaurantId}", user.Id, user.RestaurantId);
            if (user.PhoneNumber is null) user.PhoneNumber = "";
            await dbContext.AddAsync(user);
            await dbContext.SaveChangesAsync();
            return user;
        }

        public async Task<User?> GetUserAsync(User user)
        {
            logger.LogInformation("Getting user {id} for restaurant {RestaurantId}", user.Id, user.RestaurantId);
            if (user.Id == 0 && user.Email is not null)
            {
                return await dbContext.Users.Where(u => u.RestaurantId == user.RestaurantId && u.Email == user.Email)
                    .FirstOrDefaultAsync();
            }
            else if (user.Id == 0 && user.PhoneNumber is not null)
            {
                return await dbContext.Users
                    .Where(u => u.RestaurantId == user.RestaurantId && u.PhoneNumber == user.PhoneNumber)
                    .FirstOrDefaultAsync();
            }

            return await dbContext.Users.Where(u => u.Id == user.Id && u.RestaurantId == user.RestaurantId)
                .FirstOrDefaultAsync();
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            logger.LogInformation("Updating user {id} for restaurant {RestaurantId}", user.Id, user.RestaurantId);
            var existingUser = await dbContext.Users.FindAsync(user.Id) ?? throw new KeyNotFoundException($"User with id {user.Id} not found.");
            existingUser.Name = user.Name;
            existingUser.Email = user.Email;
            existingUser.PhoneNumber = user.PhoneNumber;

            logger.LogInformation("Updating user {id} for restaurant {RestaurantId}", user.Id, user.RestaurantId);

            await dbContext.SaveChangesAsync();

            return existingUser;
        }
    }
}