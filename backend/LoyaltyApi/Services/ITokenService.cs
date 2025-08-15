using LoyaltyApi.Models;

namespace LoyaltyApi.Services
{
    public interface ITokenService
    {
        Task<string> GenerateAccessTokenAsync(int customerId, int restaurantId, Role role);
        Task<string> GenerateOrGetRefreshTokenAsync(int customerId, int restaurantId, Role role);
        Task<string> GenerateForgotPasswordTokenAsync(int customerId, int restaurantId);
        Task<string> GenerateConfirmEmailTokenAsync(int customerId, int restaurantId);
        Task<bool> ValidateConfirmEmailTokenAsync(string token);
        Task<bool> ValidateForgotPasswordTokenAsync(string token);
        Task<bool> ValidateRefreshTokenAsync(string tokenValue, int restaurantId, int customerId);
        Task<(string accessTokenValue, string refreshTokenValue)> RefreshTokensAsync(int customerId, int restaurantId);
    }
}