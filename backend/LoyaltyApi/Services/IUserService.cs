using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoyaltyApi.Models;
using LoyaltyApi.RequestModels;

namespace LoyaltyApi.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByPhonenumberAsync(string phoneNumber, int restaurantId);
        Task<User?> CreateUserAsync(RegisterRequestBody registerRequestBody);
        Task<User?> GetUserByEmailAsync(string email, int restaurantId);

        Task<User?> GetUserByIdAsync(int userId, int restaurantId);
        Task<User> UpdateUserAsync(UpdateUserRequestModel requestModel, int userId, int restaurantId);
    }
}