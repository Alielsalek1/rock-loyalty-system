using LoyaltyApi.Models;

namespace LoyaltyApi.Services
{
    public interface ITokenService
    {
        Task<string> GenerateAccessTokenAsync(int customerId, int restaurantId, Role role);
        string GenerateRefreshToken(int customerId, int restaurantId, Role role);
        string GenerateForgotPasswordToken(int customerId, int restaurantId);
        string GenerateConfirmEmailToken(int customerId, int restaurantId);
        bool ValidateConfirmEmailToken(string token);
        bool ValidateForgotPasswordToken(string token);
        bool ValidateRefreshToken(string tokenValue, int restaurantId, int customerId);
        Task<(string accessTokenValue, string refreshTokenValue)> RefreshTokensAsync(int customerId, int restaurantId);
    }
}