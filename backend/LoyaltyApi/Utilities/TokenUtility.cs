using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LoyaltyApi.Models;

namespace LoyaltyApi.Utilities
{
    public class TokenUtility(ILogger<TokenUtility> logger)
    {
        public Token ReadToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var customerId = int.Parse(jwtToken.Claims.First(claim => claim.Type == "sub").Value);
            var restaurantId = int.Parse(jwtToken.Claims.First(claim => claim.Type == "restaurantId").Value);
            logger.LogInformation("Read token for customer {CustomerId} and restaurant {RestaurantId}", customerId, restaurantId);
            return new Token
            {
                CustomerId = customerId,
                RestaurantId = restaurantId,
                Role = Role.User
            };
        }
        public string GetCacheKey(TokenType tokenType, int customerId, int restaurantId)
        {
            return tokenType switch
            {
                TokenType.ConfirmEmail => $"confirm_email_token_{customerId}_{restaurantId}",
                TokenType.ForgotPasswordToken => $"forgot_password_token_{customerId}_{restaurantId}",
                _ => throw new ArgumentException($"Unsupported token type for caching: {tokenType}")
            };
        }
    }
}