using System.Security.Claims;
using System.Threading.Tasks;
using LoyaltyApi.Config;
using LoyaltyApi.Models;
using LoyaltyApi.Repositories;
using Microsoft.Extensions.Options;

namespace LoyaltyApi.Services
{
    public class TokenService(ITokenRepository repository,
    ILogger<TokenService> logger, IConfiguration configuration) : ITokenService
    {
        public async Task<string> GenerateAccessTokenAsync(int customerId, int restaurantId, Role role)
        {
            logger.LogInformation("Generating access token for customer {customerId} and restaurant {restaurantId}", customerId, restaurantId);
            Token token = new()
            {
                CustomerId = customerId,
                RestaurantId = restaurantId,
                TokenType = TokenType.AccessToken,
                Role = role
            };
            return await repository.GenerateAccessTokenAsync(token);
        }

        public async Task<string> GenerateOrGetRefreshTokenAsync(int customerId, int restaurantId, Role role)
        {
            logger.LogInformation("Generating refresh token for customer {customerId} and restaurant {restaurantId}", customerId, restaurantId);
            Token token = new()
            {
                CustomerId = customerId,
                RestaurantId = restaurantId,
                TokenType = TokenType.RefreshToken,
                Role = role
            };

            var refreshToken = await repository.GetRefreshTokenAsync(token);
            if (refreshToken != null)
            {
                logger.LogInformation("Refresh token already exists for customer {CustomerId} and restaurant {RestaurantId} and is valid", customerId, restaurantId);
                return refreshToken.TokenValue;
            }

            return await repository.GenerateRefreshTokenAsync(token);
        }

        public async Task<bool> ValidateRefreshTokenAsync(string tokenValue, int restaurantId, int customerId)
        {
            logger.LogInformation("Validating refresh token for token {tokenValue}", tokenValue);
            Token token = new()
            {
                TokenType = TokenType.RefreshToken,
                TokenValue = tokenValue,
                RestaurantId = restaurantId,
                CustomerId = customerId
            };
            return await repository.ValidateRefreshTokenAsync(token);
        }

        public async Task<(string accessTokenValue, string refreshTokenValue)> RefreshTokensAsync(int customerId, int restaurantId)
        {
            logger.LogInformation("Refreshing tokens for customer {customerId} and restaurant {restaurantId}", customerId, restaurantId);
            Token refreshToken = new()
            {
                CustomerId = customerId,
                RestaurantId = restaurantId,
                TokenType = TokenType.RefreshToken,
                Role = Role.User

            };
            Token accessToken = new()
            {
                CustomerId = customerId,
                RestaurantId = restaurantId,
                TokenType = TokenType.AccessToken,
                Role = Role.User
            };
            string refreshTokenValue = await repository.GenerateRefreshTokenAsync(refreshToken);
            string accessTokenValue = await repository.GenerateAccessTokenAsync(accessToken);
            return (accessTokenValue, refreshTokenValue);
        }

        public async Task<string> GenerateForgotPasswordTokenAsync(int customerId, int restaurantId)
        {
            logger.LogInformation("Generating forgot password token for customer {customerId} and restaurant {restaurantId}", customerId, restaurantId);
            Token token = new()
            {
                CustomerId = customerId,
                RestaurantId = restaurantId,
                TokenType = TokenType.ForgotPasswordToken,
            };
            return await repository.GenerateForgotPasswordTokenAsync(token);
        }

        public async Task<string> GenerateConfirmEmailTokenAsync(int customerId, int restaurantId)
        {
            logger.LogInformation("Generating confirm email token for customer {customerId} and restaurant {restaurantId}", customerId, restaurantId);
            Token token = new()
            {
                CustomerId = customerId,
                RestaurantId = restaurantId,
                TokenType = TokenType.ConfirmEmail,
            };
            return await repository.GenerateConfirmEmailTokenAsync(token);
        }

        public async Task<bool> ValidateConfirmEmailTokenAsync(string token)
        {
            logger.LogInformation("Validating confirm email token for token {token}", token);
            Token tokenModel = new()
            {
                TokenValue = token,
                TokenType = TokenType.ConfirmEmail
            };
            return await repository.ValidateConfirmEmailTokenAsync(tokenModel);
        }

        public async Task<bool> ValidateForgotPasswordTokenAsync(string token)
        {
            logger.LogInformation("Validating forgot password token for token {token}", token);
            Token tokenModel = new()
            {
                TokenValue = token,
                TokenType = TokenType.ForgotPasswordToken
            };
            return await repository.ValidateForgotPasswordTokenAsync(tokenModel);
        }
    }
}