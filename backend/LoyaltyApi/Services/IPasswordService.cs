using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoyaltyApi.Models;

namespace LoyaltyApi.Services
{
    public interface IPasswordService
    {
        Task<Password?> GetPasswordByCustomerIdAsync(int customerId, int restaurantId);
        Task<Password?> GetAndValidatePasswordAsync(int customerId, int restaurantId, string inputPassword);
        Task<Password> CreatePasswordAsync(int customerId, int restaurantId, string password);
        Task<Password> UpdatePasswordAsync(int customerId, int restaurantId, string password);
        Task ConfirmEmail(string token);
        Task<Password> UnConfirmEmail(int customerId, int restaurantId);
    }
}