using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using LoyaltyApi.Config;
using LoyaltyApi.Data;
using LoyaltyApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Caching.Memory; // Add this using
using Sprache;
using LoyaltyApi.Utilities;

namespace LoyaltyApi.Repositories
{
    public class TokenRepository(
        FrontendDbContext dbContext,
        IOptions<JwtOptions> jwtOptions,
        IMemoryCache memoryCache,
        TokenUtility tokenUtility,
        ILogger<TokenRepository> logger) : ITokenRepository
    {


        private void StoreTokenInCache(Token token, TimeSpan? expiration = null)
        {
            var cacheKey = tokenUtility.GetCacheKey(token.TokenType, token.CustomerId, token.RestaurantId);
            var cacheExpiration = expiration ?? TimeSpan.FromMinutes(10);
            memoryCache.Set(cacheKey, token, cacheExpiration);

            logger.LogInformation("Stored {TokenType} token in cache with key: {CacheKey}",
                token.TokenType, cacheKey);
        }

        private Token? GetTokenFromCache(TokenType tokenType, int customerId, int restaurantId)
        {
            var cacheKey = tokenUtility.GetCacheKey(tokenType, customerId, restaurantId);
            var exists = memoryCache.TryGetValue(cacheKey, out Token? cachedToken);

            logger.LogInformation("Cache lookup for key {CacheKey}: {Found}",
                cacheKey, exists ? "Found" : "Not Found");

            return exists ? cachedToken : null;
        }

        // Generic method for memory-cached token generation
        private string GenerateMemoryCachedToken(Token token)
        {
            JwtSecurityToken generatedToken = GenerateToken(token);
            var tokenHandler = new JwtSecurityTokenHandler();
            string valueToken = tokenHandler.WriteToken(generatedToken);

            var jwtToken = tokenHandler.ReadJwtToken(valueToken);
            int subject = int.Parse(jwtToken.Claims.First(c => c.Type == "sub").Value);
            int restaurantId = int.Parse(jwtToken.Claims.First(c => c.Type == "restaurantId").Value);

            var tokenToStore = new Token
            {
                TokenValue = valueToken,
                CustomerId = subject,
                RestaurantId = restaurantId,
                TokenType = token.TokenType
            };

            // Store in memory cache instead of database
            StoreTokenInCache(tokenToStore);

            logger.LogInformation(
                "Generated {TokenType} token for customer {CustomerId} and restaurant {RestaurantId}",
                token.TokenType, subject, restaurantId);

            return valueToken;
        }

        // Generic method for memory-cached token validation
        private bool ValidateMemoryCachedToken(Token token)
        {
            logger.LogInformation(
                "Validating {TokenType} token for customer {CustomerId} and restaurant {RestaurantId}",
                token.TokenType, token.CustomerId, token.RestaurantId);

            // First validate JWT structure and expiration
            var isTokenValid = ValidateToken(token);
            if (!isTokenValid) return false;

            // Check memory cache
            var cachedToken = GetTokenFromCache(token.TokenType, token.CustomerId, token.RestaurantId);
            return cachedToken?.TokenValue == token.TokenValue;
        }

        public async Task<string> GenerateAccessTokenAsync(Token token)
        {
            JwtSecurityToken generatedToken = GenerateToken(token);
            logger.LogInformation("Generated access token for customer {CustomerId} and restaurant {RestaurantId}",
                token.CustomerId, token.RestaurantId);
            return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(generatedToken));
        }

        private JwtSecurityToken GenerateToken(Token token)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, token.CustomerId.ToString()),
                new Claim("restaurantId", token.RestaurantId.ToString()),
                new Claim("role", token.Role.ToString())
            };
            var signingKey = jwtOptions.Value.SigningKey.ToString();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var generatedToken = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: token.TokenType != TokenType.RefreshToken
                    ? DateTime.Now.AddMinutes(jwtOptions.Value.ExpirationInMinutes)
                    : DateTime.Now.AddMonths(6),
                signingCredentials: creds
            );
            return generatedToken;
        }

        // Cookie-only approach: Synchronous validation (no database checks)
        public bool ValidateRefreshToken(Token token)
        {
            logger.LogInformation("Validating refresh token for customer {CustomerId} and restaurant {RestaurantId}",
                token.CustomerId, token.RestaurantId);

            // For cookie-only approach, just validate JWT structure and expiration
            return ValidateToken(token);
        }

        public bool ValidateToken(Token token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtOptions.Value.SigningKey);
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
            try
            {
                tokenHandler.ValidateToken(token.TokenValue, validationParameters, out SecurityToken validatedToken);
                return validatedToken != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Token?> GetRefreshTokenAsync(Token token)
        {
            var refreshToken = await dbContext.Tokens.FirstOrDefaultAsync(t =>
                t.CustomerId == token.CustomerId && t.RestaurantId == token.RestaurantId &&
                t.TokenType == TokenType.RefreshToken);

            if (refreshToken == null)
                return null;

            if (ValidateToken(refreshToken) == true)
                return refreshToken;

            return null;
        }

        public string GenerateRefreshToken(Token token)
        {
            JwtSecurityToken generatedToken = GenerateToken(token);
            var tokenHandler = new JwtSecurityTokenHandler();
            string valueToken = tokenHandler.WriteToken(generatedToken).ToString();
            int subject = int.Parse(tokenHandler.ReadJwtToken(valueToken).Claims.First(claim => claim.Type == "sub")
                .Value);
            DateTime expiration = tokenHandler.ReadJwtToken(valueToken).ValidTo;
            int restaurantId = int.Parse(tokenHandler.ReadJwtToken(valueToken).Claims
                .First(claim => claim.Type == "restaurantId").Value);
            var refreshToken = new Token
            {
                TokenValue = valueToken,
                CustomerId = subject,
                RestaurantId = restaurantId,
                TokenType = TokenType.RefreshToken
            };
            logger.LogInformation("Generated refresh token for customer {CustomerId} and restaurant {RestaurantId}",
                token.CustomerId, token.RestaurantId);
            return refreshToken.TokenValue;
        }

        // Updated methods using memory cache
        public string GenerateForgotPasswordToken(Token token)
        {
            return GenerateMemoryCachedToken(token);
        }

        public string GenerateConfirmEmailToken(Token token)
        {
            return GenerateMemoryCachedToken(token);
        }

        public bool ValidateConfirmEmailToken(Token token)
        {
            return ValidateMemoryCachedToken(token);
        }

        public bool ValidateForgotPasswordToken(Token token)
        {
            return ValidateMemoryCachedToken(token);
        }

        // Debug methods for cache inspection
        public bool IsCacheContainsToken(TokenType tokenType, int customerId, int restaurantId)
        {
            var cacheKey = tokenUtility.GetCacheKey(tokenType, customerId, restaurantId);
            var exists = memoryCache.TryGetValue(cacheKey, out _);

            logger.LogInformation("Cache check for {TokenType}: Key={CacheKey}, Exists={Exists}",
                tokenType, cacheKey, exists);

            return exists;
        }

        public Token? GetCachedTokenForInspection(TokenType tokenType, int customerId, int restaurantId)
        {
            var cachedToken = GetTokenFromCache(tokenType, customerId, restaurantId);

            if (cachedToken != null)
            {
                logger.LogInformation("Found cached token: Type={TokenType}, CustomerId={CustomerId}, RestaurantId={RestaurantId}, TokenValue={TokenPreview}...",
                    cachedToken.TokenType, cachedToken.CustomerId, cachedToken.RestaurantId,
                    cachedToken.TokenValue[..Math.Min(20, cachedToken.TokenValue.Length)]); // First 20 characters or less
            }

            return cachedToken;
        }


    }
}