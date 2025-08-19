using LoyaltyApi.Models;

namespace LoyaltyApi.Repositories
{
    public interface ITokenRepository
    {
        Task<string> GenerateAccessTokenAsync(Token token);

        Task<string> GenerateRefreshTokenAsync(Token token);

        Task<bool> ValidateTokenAsync(Token token);

        Task<bool> ValidateRefreshTokenAsync(Token token);

        Task<bool> ValidateConfirmEmailTokenAsync(Token token);

        string GenerateForgotPasswordToken(Token token);

        string GenerateConfirmEmailToken(Token token);

        Task<bool> ValidateForgotPasswordTokenAsync(Token token);

        Task<Token?> GetRefreshTokenAsync(Token token);

        bool IsCacheContainsToken(TokenType tokenType, int customerId, int restaurantId);

        Token? GetCachedTokenForInspection(TokenType tokenType, int customerId, int restaurantId);
    }
}