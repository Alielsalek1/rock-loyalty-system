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
using Sprache;

namespace LoyaltyApi.Repositories
{
    public class TokenRepository(
        RockDbContext dbContext,
        IOptions<JwtOptions> jwtOptions,
        ILogger<TokenRepository> logger) : ITokenRepository
    {
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

        public async Task<bool> ValidateRefreshTokenAsync(Token token)
        {
            logger.LogInformation("Validating refresh token for customer {CustomerId} and restaurant {RestaurantId}",
                token.CustomerId, token.RestaurantId);
            var isTokenValid = await ValidateTokenAsync(token);
            var tokenExistsInDb = await dbContext.Tokens.AnyAsync(t =>
                t.CustomerId == token.CustomerId && t.RestaurantId == token.RestaurantId &&
                t.TokenValue == token.TokenValue && t.TokenType == TokenType.RefreshToken);
            
            return isTokenValid && tokenExistsInDb;
        }

        public async Task<bool> ValidateTokenAsync(Token token)
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
                return await Task.FromResult(validatedToken != null);
            }
            catch
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<Token?> GetRefreshTokenAsync(Token token)
        {
            var refreshToken = await dbContext.Tokens.FirstOrDefaultAsync(t =>
                t.CustomerId == token.CustomerId && t.RestaurantId == token.RestaurantId &&
                t.TokenType == TokenType.RefreshToken);

            if (refreshToken == null)
                return null;

            if (await ValidateTokenAsync(refreshToken) == true)
                return refreshToken;

            return null;
        }

        public async Task<string> GenerateRefreshTokenAsync(Token token)
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
            await dbContext.Tokens.AddAsync(refreshToken);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Generated refresh token for customer {CustomerId} and restaurant {RestaurantId}",
                token.CustomerId, token.RestaurantId);
            return refreshToken.TokenValue;
        }

        public async Task<string> GenerateForgotPasswordTokenAsync(Token token)
        {
            JwtSecurityToken generatedToken = GenerateToken(token);
            var tokenHandler = new JwtSecurityTokenHandler();
            string valueToken = tokenHandler.WriteToken(generatedToken).ToString();
            int subject = int.Parse(tokenHandler.ReadJwtToken(valueToken).Claims.First(claim => claim.Type == "sub")
                .Value);
            DateTime expiration = tokenHandler.ReadJwtToken(valueToken).ValidTo;
            int restaurantId = int.Parse(tokenHandler.ReadJwtToken(valueToken).Claims
                .First(claim => claim.Type == "restaurantId").Value);
            var forgotPasswordToken = new Token
            {
                TokenValue = valueToken,
                CustomerId = subject,
                RestaurantId = restaurantId,
                TokenType = TokenType.ForgotPasswordToken
            };
            await dbContext.Tokens.AddAsync(forgotPasswordToken);
            await dbContext.SaveChangesAsync();
            logger.LogInformation(
                "Generated forgot password token for customer {CustomerId} and restaurant {RestaurantId}",
                token.CustomerId, token.RestaurantId);
            return forgotPasswordToken.TokenValue;
        }

        public async Task<string> GenerateConfirmEmailTokenAsync(Token token)
        {
            JwtSecurityToken generatedToken = GenerateToken(token);
            var tokenHandler = new JwtSecurityTokenHandler();
            string valueToken = tokenHandler.WriteToken(generatedToken).ToString();
            int subject = int.Parse(tokenHandler.ReadJwtToken(valueToken).Claims.First(claim => claim.Type == "sub")
                .Value);
            DateTime expiration = tokenHandler.ReadJwtToken(valueToken).ValidTo;
            int restaurantId = int.Parse(tokenHandler.ReadJwtToken(valueToken).Claims
                .First(claim => claim.Type == "restaurantId").Value);
            var confirmEmailToken = new Token
            {
                TokenValue = valueToken,
                CustomerId = subject,
                RestaurantId = restaurantId,
                TokenType = TokenType.ConfirmEmail
            };
            await dbContext.Tokens.AddAsync(confirmEmailToken);
            await dbContext.SaveChangesAsync();
            logger.LogInformation(
                "Generated confirm email token for customer {CustomerId} and restaurant {RestaurantId}",
                token.CustomerId, token.RestaurantId);
            return confirmEmailToken.TokenValue;
        }

        public async Task<bool> ValidateConfirmEmailTokenAsync(Token token)
        {
            logger.LogInformation(
                "Validating confirm email token for customer {CustomerId} and restaurant {RestaurantId}",
                token.CustomerId, token.RestaurantId);
            var isTokenValid = await ValidateTokenAsync(token);
            var tokenExistsInDb = await dbContext.Tokens.AnyAsync(t =>
                t.TokenValue == token.TokenValue && t.TokenType == TokenType.ConfirmEmail);
            
            return isTokenValid && tokenExistsInDb;
        }

        public async Task<bool> ValidateForgotPasswordTokenAsync(Token token)
        {
            logger.LogInformation(
                "Validating forgot password token for customer {CustomerId} and restaurant {RestaurantId}",
                token.CustomerId, token.RestaurantId);
            var isTokenValid = await ValidateTokenAsync(token);
            var tokenExistsInDb = await dbContext.Tokens.AnyAsync(t =>
                t.TokenValue == token.TokenValue && t.TokenType == TokenType.ForgotPasswordToken);
            
            return isTokenValid && tokenExistsInDb;
        }
    }
}