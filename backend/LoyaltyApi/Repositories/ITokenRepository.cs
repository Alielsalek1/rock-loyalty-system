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
        
        Task<string> GenerateForgotPasswordTokenAsync(Token token);

        Task<string> GenerateConfirmEmailTokenAsync(Token token);
        
        Task<bool> ValidateForgotPasswordTokenAsync(Token token);

        Task<Token?> GetRefreshTokenAsync(Token token);
    }
}