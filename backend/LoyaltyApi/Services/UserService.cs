using System.Security.Claims;
using LoyaltyApi.Exceptions;
using LoyaltyApi.Models;
using LoyaltyApi.Repositories;
using LoyaltyApi.RequestModels;
using Microsoft.AspNetCore.Identity;

namespace LoyaltyApi.Services
{
    public class UserService(IUserRepository userRepository,
        IPasswordService passwordService,
        ILogger<UserService> logger) : IUserService
    {
        public async Task<User?> CreateUserAsync(RegisterRequestBody registerRequestBody)
        {
            logger.LogInformation("Creating user {Name}", registerRequestBody.Name);
            User? user = new()
            {
                Name = registerRequestBody.Name,
                Email = registerRequestBody.Email,
                PhoneNumber = registerRequestBody.PhoneNumber,
                RestaurantId = registerRequestBody.RestaurantId
            };
            user = await userRepository.CreateUserAsync(user);
            
            if (user == null) return null;
            
            if (registerRequestBody.Password is not null)
                await passwordService.CreatePasswordAsync(user.Id, user.RestaurantId, registerRequestBody.Password);
                
            return user;
        }

        public async Task<User?> GetUserByEmailAsync(string email, int restaurantId)
        {
            logger.LogInformation("Getting user {Email}", email);
            User user = new()
            {
                Email = email,
                RestaurantId = restaurantId
            };
            return await userRepository.GetUserAsync(user);
        }

        public async Task<User?> GetUserByIdAsync(int userId, int restaurantId)
        {
            logger.LogInformation("Getting user with id");
            User user = new()
            {
                Id = userId,
                RestaurantId = restaurantId
            };
            return await userRepository.GetUserAsync(user);
        }

        public async Task<User?> GetUserByPhonenumberAsync(string phoneNumber, int restaurantId)
        {
            logger.LogInformation("Getting user with phone number {phoneNumber} and restaurantId {restaurantId}", phoneNumber, restaurantId);
            User user = new()
            {
                PhoneNumber = phoneNumber,
                RestaurantId = restaurantId
            };
            return await userRepository.GetUserAsync(user);
        }

        public async Task<User> UpdateUserAsync(UpdateUserRequestModel requestModel, int userId, int restaurantId)
        {
            logger.LogInformation("getting user with id {userId} and restaurantId {restaurantId}", userId, restaurantId);

            User? pastUser = await userRepository.GetUserAsync(new User { Id = userId, RestaurantId = restaurantId })
                ?? throw new NotFoundException("User not found");

            logger.LogInformation("Updating user {Name}", requestModel.Name);
            User user = new()
            {
                Name = requestModel.Name ?? pastUser.Name,
                Email = requestModel.Email ?? pastUser.Email,
                PhoneNumber = requestModel.PhoneNumber ?? pastUser.PhoneNumber,
                Id = userId,
                RestaurantId = restaurantId
            };
            return await userRepository.UpdateUserAsync(user);
        }
    }
}