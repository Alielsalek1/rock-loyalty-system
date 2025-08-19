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
    ILogger<TokensController> logger,
    ITokenRepository tokenRepository) : ControllerBase
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

        if (!await tokenService.ValidateRefreshTokenAsync(HttpContext.Request.Cookies["refreshToken"], restaurantId, userId))
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
    //TODO:Testing shit don't bother me
    [HttpGet("debug/cache-tokens/{customerId}/{restaurantId}")]
    public async Task<IActionResult> GenerateConfirmEmailToken(int customerId, int restaurantId)
    {
        var token = new Token
        {
            CustomerId = customerId,
            RestaurantId = restaurantId,
            TokenType = TokenType.ConfirmEmail,
            Role = Role.User
        };

        var tokenValue = await tokenRepository.GenerateConfirmEmailTokenAsync(token);

        return Ok(new { TokenValue = tokenValue });
    }
}