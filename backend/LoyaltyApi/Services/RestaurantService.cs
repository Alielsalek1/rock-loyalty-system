using LoyaltyApi.Exceptions;
using LoyaltyApi.Models;
using LoyaltyApi.Repositories;
using LoyaltyApi.RequestModels;
namespace LoyaltyApi.Services
{
    public class RestaurantService(IRestaurantRepository repository,
    ILogger<RestaurantService> logger) : IRestaurantService
    {
        //Get Methods
        public Task<Restaurant?> GetRestaurantById(int restaurantId)
        {
            logger.LogInformation("Getting restaurant info for restaurant {restaurantId}", restaurantId);
            return repository.GetRestaurantById(restaurantId);
        }
        public async Task<double?> GetCreditPointBuyingRate(int restaurantId)
        {
            logger.LogInformation("Getting credit point buying rate for restaurant {restaurantId}", restaurantId);
            var result = await repository.GetRestaurantById(restaurantId);
            if (result == null) return null;
            return result.CreditPointsBuyingRate;
        }

        public async Task<double?> GetCreditPointSellingRate(int restaurantId)
        {
            logger.LogInformation("Getting credit point selling rate for restaurant {restaurantId}", restaurantId);
            var result = await repository.GetRestaurantById(restaurantId);
            if (result == null) return null;
            return result.CreditPointsSellingRate;
        }

        public async Task<int?> GetVoucherLifeTime(int restaurantId)
        {
            logger.LogInformation("Getting voucher life time for restaurant {restaurantId}", restaurantId);
            var result = await repository.GetRestaurantById(restaurantId);
            if (result == null) return null;

            return result.VoucherLifeTime;
        }


        //Update Methods
        public async Task UpdateRestaurantInfo(int restaurantId, RestaurantCreditPointsRequestModel restaurantRequestModel)
        {
            logger.LogInformation("Updating restaurant info for restaurant {restaurantId}", restaurantId);
            Restaurant existingRestaurant = await repository.GetRestaurantById(restaurantId) ?? throw new Exception("Invalid restaurant");
            existingRestaurant.CreditPointsBuyingRate = restaurantRequestModel.CreditPointsBuyingRate ?? existingRestaurant.CreditPointsBuyingRate;
            existingRestaurant.CreditPointsSellingRate = restaurantRequestModel.CreditPointsSellingRate ?? existingRestaurant.CreditPointsSellingRate;
            existingRestaurant.VoucherLifeTime = restaurantRequestModel.VoucherLifeTime ?? existingRestaurant.VoucherLifeTime;
            existingRestaurant.CreditPointsLifeTime = restaurantRequestModel.CreditPointsLifeTime ?? existingRestaurant.CreditPointsLifeTime;
            await repository.UpdateRestaurant(existingRestaurant);
        }

        //Create Methods
        public async Task CreateRestaurant(CreateRestaurantRequestModel createRestaurant)
        {
            logger.LogInformation("Creating restaurant with id {restaurantId}", createRestaurant.RestaurantId);

            if (await repository.GetRestaurantById(createRestaurant.RestaurantId) != null)
                throw new AlreadyExistsException("Restaurant already exists");

            Restaurant restaurant = new()
            {
                RestaurantId = createRestaurant.RestaurantId,
                CreditPointsBuyingRate = createRestaurant.CreditPointsBuyingRate,
                CreditPointsSellingRate = createRestaurant.CreditPointsSellingRate,
                CreditPointsLifeTime = createRestaurant.CreditPointsLifeTime,
                VoucherLifeTime = createRestaurant.VoucherLifeTime
            };

            await repository.CreateRestaurant(restaurant);
        }

    }
}