using LoyaltyApi.Config;
using LoyaltyApi.Models;
using LoyaltyApi.RequestModels;
using LoyaltyApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LoyaltyApi.Controllers;

/// <summary>
/// Controller to handle Facebook OAuth2 authentication.
/// </summary>
[ApiController]
[Route("api/oauth2/")]
public class FacebookOAuth2Controller(OAuth2Service oauth2Service,
ILogger<FacebookOAuth2Controller> logger,
IUserService userService,
ITokenService tokenService,
IOptions<JwtOptions> jwtOptions) : ControllerBase
{
    /// <summary>
    /// Handles Facebook OAuth2 sign-in and creates or authenticates a user.
    /// </summary>
    /// <param name="body">The OAuth2 body containing the Facebook access token and restaurant ID.</param>
    /// <returns>
    /// An ActionResult containing user information and JWT tokens if successful.
    /// </returns>
    /// <response code="200">User authenticated successfully.</response>
    /// <response code="400">Invalid request body or Facebook token.</response>
    /// <response code="500">Server error during authentication.</response>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/oauth2/signin-facebook
    ///     {
    ///         "accessToken": "facebook_access_token_here",
    ///         "restaurantId": 1
    ///     }
    ///
    /// Sample response:
    ///
    ///     200 OK
    ///     {
    ///         "success": true,
    ///         "message": "Login successful",
    ///         "data": {
    ///             "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    ///             "user": {
    ///                 "id": 1,
    ///                 "name": "John Doe",
    ///                 "email": "john.doe@example.com",
    ///                 "restaurantId": 1
    ///             }
    ///         }
    ///     }
    /// 
    /// The refresh token is set as an HTTP-only cookie.
    /// </remarks>
    [HttpPost("signin-facebook")]
    public async Task<ActionResult> SignInWithFacebook([FromBody] OAuth2Body body)
    {
        var user = await oauth2Service.HandleFacebookSignIn(body.AccessToken);
        var existingUser = await userService.GetUserByEmailAsync(user.Email, body.RestaurantId);
        if (existingUser is null)
        {
            var registerBody = new RegisterRequestBody()
            {
                Email = user.Email,
                Name = user.Name,
                RestaurantId = body.RestaurantId
            };
            existingUser = await userService.CreateUserAsync(registerBody) ?? throw new HttpRequestException("Failed to create user.");
        }
        string accessToken = await tokenService.GenerateAccessTokenAsync(existingUser.Id, existingUser.RestaurantId, Role.User);

        HttpContext.Response.Cookies.Delete("refreshToken");

        string refreshToken = tokenService.GenerateRefreshToken(existingUser.Id, existingUser.RestaurantId, Role.User);
        HttpContext.Response.Cookies.Append("refreshToken", refreshToken, jwtOptions.Value.JwtCookieOptions);

        return Ok(new
        {
            success = true,
            message = "Login successful",
            data = new
            {
                accessToken,
                user = existingUser
            }
        });
    }
}
