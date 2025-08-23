using LoyaltyApi.Models;

namespace LoyaltyApi.Repositories
{
    public interface ITokenRepository
    {
        Task<string> GenerateAccessTokenAsync(Token token);

        string GenerateRefreshToken(Token token);

        bool ValidateToken(Token token);

        bool ValidateRefreshToken(Token token);

        bool ValidateConfirmEmailToken(Token token);

        string GenerateForgotPasswordToken(Token token);

        string GenerateConfirmEmailToken(Token token);

        bool ValidateForgotPasswordToken(Token token);

        Task<Token?> GetRefreshTokenAsync(Token token);

        bool IsCacheContainsToken(TokenType tokenType, int customerId, int restaurantId);

        Token? GetCachedTokenForInspection(TokenType tokenType, int customerId, int restaurantId);
    }
}