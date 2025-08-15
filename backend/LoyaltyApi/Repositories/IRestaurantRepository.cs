using LoyaltyApi.Models;
using LoyaltyApi.RequestModels;

namespace LoyaltyApi.Repositories
{
    public interface IRestaurantRepository
    {
        Task<Restaurant?> GetRestaurantById(int restaurantId);

        Task<IEnumerable<Restaurant>> GetAllRestaurantsAsync();

        //Update Methods
        Task UpdateRestaurant(Restaurant restaurant);


        //Create Method 
        Task CreateRestaurant(Restaurant restaurant);
    }
}