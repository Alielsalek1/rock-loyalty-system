using System.Security.Claims;
using LoyaltyApi.Exceptions;
using LoyaltyApi.Models;
using LoyaltyApi.Repositories;
using LoyaltyApi.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace LoyaltyApi.Services
{
    public class PasswordService(IPasswordRepository repository,
        IPasswordHasher<Password> passwordHasher,
        ITokenService tokenService,
        TokenUtility tokenUtility,
        ILogger<PasswordService> logger) : IPasswordService
    {
        public async Task<Password?> GetPasswordByCustomerIdAsync(int customerId, int restaurantId)
        {
            Password password = new()
            {
                CustomerId = customerId,
                RestaurantId = restaurantId
            };
            return await repository.GetPasswordAsync(password);
        }
        public async Task ConfirmEmail(string token)
        {
            logger.LogInformation("Confirming email with token {token}", token);
            if (!await tokenService.ValidateConfirmEmailTokenAsync(token)) throw new SecurityTokenMalformedException("Invalid Confirm Email Token");
            Token tokenData = tokenUtility.ReadToken(token);
            Password passwordModel = new()
            {
                CustomerId = tokenData.CustomerId,
                RestaurantId = tokenData.RestaurantId,
            };
            Password? password = await repository.GetPasswordAsync(passwordModel) ?? throw new Exception("Password not found");
            if (password.ConfirmedEmail) throw new EmailAlreadyConfirmedException("Email already confirmed");
            password.ConfirmedEmail = true;
            await repository.UpdatePasswordAsync(password);
        }

        public async Task<Password> CreatePasswordAsync(int customerId, int restaurantId, string password)
        {
            logger.LogInformation("Creating password for customer {customerId} and restaurant {restaurantId}", customerId, restaurantId);
            if (password is null) throw new ArgumentException("Password cannot be null");
            Password passwordModel = new()
            {
                CustomerId = customerId,
                RestaurantId = restaurantId,
            };
            string hashedPassword = passwordHasher.HashPassword(passwordModel, password);
            passwordModel.Value = hashedPassword;
            return await repository.CreatePasswordAsync(passwordModel);
        }

        public async Task<Password?> GetAndValidatePasswordAsync(int customerId, int restaurantId, string inputPassword)
        {
            logger.LogInformation("Getting and validating password for customer {customerId} and restaurant {restaurantId}", customerId, restaurantId);
            Password passwordModel = new()
            {
                CustomerId = customerId,
                RestaurantId = restaurantId
            };
            Password? password = await repository.GetPasswordAsync(passwordModel);
            if (password is null) return null;
            if (!VerifyPassword(password, inputPassword)) return null;
            logger.LogInformation("Password validated successfully for customer {customerId}", customerId);
            password.ConfirmedEmail = true;
            return password;
        }

        public async Task<Password> UnConfirmEmail(int customerId, int restaurantId)
        {
            logger.LogInformation("Unconfirming email for customer {customerId} and restaurant {restaurantId}", customerId, restaurantId);
            Password passwordModel = new()
            {
                CustomerId = customerId,
                RestaurantId = restaurantId
            };
            return await repository.UpdatePasswordAsync(passwordModel);
        }

        public async Task<Password> UpdatePasswordAsync(int customerId, int restaurantId, string password)
        {
            Password passwordModel = new()
            {
                CustomerId = customerId,
                RestaurantId = restaurantId
            };
            
            // Get the existing tracked entity
            Password existingPassword = await repository.GetPasswordAsync(passwordModel) ?? throw new ArgumentException("Password doesn't exist");
            
            // Update the tracked entity's properties
            string hashedPassword = passwordHasher.HashPassword(existingPassword, password);
            existingPassword.Value = hashedPassword;
            existingPassword.ConfirmedEmail = true;

            logger.LogInformation("updating password in DB");
            return await repository.UpdatePasswordAsync(existingPassword);
        }
        private bool VerifyPassword(Password password, string providedPassword)
        {
            logger.LogInformation("Verifying password");
            var result = passwordHasher.VerifyHashedPassword(password, password.Value, providedPassword) != PasswordVerificationResult.Failed;
            logger.LogInformation("Password verification result: {Result}", result);
            return result;
        }
    }
}
