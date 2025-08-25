using LoyaltyApi.Data;
using LoyaltyApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyApi.Repositories
{
    public class RestaurantRepository(RockDbContext dbContext,
    ILogger<RestaurantRepository> logger) : IRestaurantRepository
    {
        //Create Method
        public async Task CreateRestaurant(Restaurant restaurant)
        {
            await dbContext.Restaurants.AddAsync(restaurant);

            await dbContext.SaveChangesAsync();

            logger.LogInformation("Restaurant {RestaurantId} created successfully", restaurant.RestaurantId);
        }
        //Get Method
        public async Task<Restaurant?> GetRestaurantById(int restaurantId)
        {
            var restaurant = await dbContext.Restaurants.FirstOrDefaultAsync(r => r.RestaurantId == restaurantId);
            logger.LogInformation("Getting restaurant {RestaurantId}", restaurantId);
            return restaurant;
        }

        public async Task<IEnumerable<Restaurant>> GetAllRestaurantsAsync()
        {
            logger.LogInformation("Getting all restaurants");
            return await dbContext.Restaurants.ToListAsync();
        }
        //Update Method
        public async Task UpdateRestaurant(Restaurant restaurant)
        {

            dbContext.Restaurants.Update(restaurant);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Updating restaurant {RestaurantId}", restaurant.RestaurantId);

        }

    }

}
