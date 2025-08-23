using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LoyaltyApi.Config;
using LoyaltyApi.Models;
using LoyaltyApi.Repositories;
using LoyaltyApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LoyaltyApi.Controllers;

/// <summary>
///  Controller for managing token-related operations.
/// </summary>
[ApiController]
[Route("api/tokens")]
public class TokensController(
    ITokenService tokenService,
    IOptions<JwtOptions> jwtOptions,
    ILogger<TokensController> logger) : ControllerBase
{
    /// <summary>
    /// Refreshes the access and refresh tokens.
    /// </summary>
    /// <remarks>
    /// 
    /// Sample request:
    ///     PUT /api/tokens/refresh-tokens
    ///
    /// Sample response:
    ///     200 OK
    ///     {
    ///         "success": true,
    ///         "message": "Tokens refreshed",
    ///         "data": {
    ///             "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    ///         }
    ///     }
    /// 
    /// Authorization header with JWT Bearer token is required.
    /// </remarks>
    /// <returns> Access Token in the response and Refresh Token is set in a cookie.</returns>
    /// <response code="200">If the tokens are refreshed successfully.</response>
    /// <response code="400">If the refresh token is invalid.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="500">If any other exception occurs.</response>
    [HttpPut]
    [Route("refresh-tokens")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult> RefreshTokens()
    {
        logger.LogInformation("Refresh tokens request for user {UserId}",
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        string userClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException();
        string restaurantClaim = User.FindFirst("restaurantId")?.Value ?? throw new UnauthorizedAccessException();
        _ = int.TryParse(userClaim, out var userId);
        _ = int.TryParse(restaurantClaim, out var restaurantId);

        logger.LogInformation("refreshing for user {UserId} and restaurant {RestaurantId}", userId, restaurantId);

        if (!tokenService.ValidateRefreshToken(HttpContext.Request.Cookies["refreshToken"]!, restaurantId, userId))
            return Unauthorized(new { success = false, message = "Invalid refresh token" });

        logger.LogInformation("Validated the refresh token");

        var (accessToken, refreshToken) = await tokenService.RefreshTokensAsync(userId, restaurantId);
        HttpContext.Response.Cookies.Append("refreshToken", refreshToken, jwtOptions.Value.JwtCookieOptions);
        return Ok(new
        {
            success = true,
            message = "Tokens refreshed",
            data = new { accessToken }
        });
    }

    //TODO: For Testing 
    // [HttpGet]
    // [Route("debug/cache-check")]
    // public async Task<ActionResult> CheckTokenInCache([FromQuery] string tokenType, [FromQuery] int customerId, [FromQuery] int restaurantId)
    // {
    //     logger.LogInformation("Debug: Checking cache for {TokenType} token - Customer: {CustomerId}, Restaurant: {RestaurantId}",
    //         tokenType, customerId, restaurantId);

    //     if (!Enum.TryParse<TokenType>(tokenType, true, out var tokenTypeEnum))
    //     {
    //         return BadRequest(new
    //         {
    //             success = false,
    //             message = $"Invalid token type. Valid types: {string.Join(", ", Enum.GetNames<TokenType>())}"
    //         });
    //     }

    //     try
    //     {
    //         var existsInCache = await tokenRepository.IsCacheContainsTokenAsync(tokenTypeEnum, customerId, restaurantId);
    //         var cachedToken = await tokenRepository.GetCachedTokenForInspectionAsync(tokenTypeEnum, customerId, restaurantId);

    //         return Ok(new
    //         {
    //             success = true,
    //             message = "Cache check completed",
    //             data = new
    //             {
    //                 tokenType = tokenTypeEnum.ToString(),
    //                 customerId,
    //                 restaurantId,
    //                 existsInCache,
    //                 tokenInfo = cachedToken != null ? new
    //                 {
    //                     tokenType = cachedToken.TokenType.ToString(),
    //                     customerId = cachedToken.CustomerId,
    //                     restaurantId = cachedToken.RestaurantId,
    //                     role = cachedToken.Role.ToString(),
    //                     tokenValuePreview = cachedToken.TokenValue?.Substring(0, Math.Min(20, cachedToken.TokenValue.Length)) + "..." // Show first 20 chars only
    //                 } : null
    //             }
    //         });
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.LogError(ex, "Error checking cache for token");
    //         return StatusCode(500, new
    //         {
    //             success = false,
    //             message = "Error checking cache",
    //             error = ex.Message
    //         });
    //     }
    // }

    // /// <summary>
    // /// Debug endpoint to get all cached tokens information (without token values for security).
    // /// </summary>
    // /// <remarks>
    // /// Sample request:
    // ///     GET /api/tokens/debug/cache-summary
    // /// </remarks>
    // /// <returns>Summary of all tokens currently in cache</returns>
    // /// <response code="200">Returns cache summary</response>
    // [HttpGet]
    // [Route("debug/cache-summary")]
    // public async Task<ActionResult> GetCacheSummary()
    // {
    //     logger.LogInformation("Debug: Getting cache summary");

    //     try
    //     {
    //         var tokenTypes = new[] { TokenType.ConfirmEmail, TokenType.ForgotPasswordToken };
    //         var cacheSummary = new List<object>();

    //         // This is a simple approach - in production you might want to track cached tokens differently
    //         // For now, we'll just check some common scenarios
    //         for (int customerId = 1; customerId <= 10; customerId++)
    //         {
    //             for (int restaurantId = 1; restaurantId <= 5; restaurantId++)
    //             {
    //                 foreach (var tokenType in tokenTypes)
    //                 {
    //                     var existsInCache = await tokenRepository.IsCacheContainsTokenAsync(tokenType, customerId, restaurantId);
    //                     if (existsInCache)
    //                     {
    //                         var cachedToken = await tokenRepository.GetCachedTokenForInspectionAsync(tokenType, customerId, restaurantId);
    //                         if (cachedToken != null)
    //                         {
    //                             cacheSummary.Add(new
    //                             {
    //                                 tokenType = cachedToken.TokenType.ToString(),
    //                                 customerId = cachedToken.CustomerId,
    //                                 restaurantId = cachedToken.RestaurantId,
    //                                 role = cachedToken.Role.ToString()
    //                             });
    //                         }
    //                     }
    //                 }
    //             }
    //         }

    //         return Ok(new
    //         {
    //             success = true,
    //             message = "Cache summary retrieved",
    //             data = new
    //             {
    //                 totalCachedTokens = cacheSummary.Count,
    //                 tokens = cacheSummary,
    //                 note = "Only showing first 50 customer/restaurant combinations (1-10 customers, 1-5 restaurants)"
    //             }
    //         });
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.LogError(ex, "Error getting cache summary");
    //         return StatusCode(500, new
    //         {
    //             success = false,
    //             message = "Error getting cache summary",
    //             error = ex.Message
    //         });
    //     }
    // }


}