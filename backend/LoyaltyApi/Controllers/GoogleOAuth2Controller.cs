using LoyaltyApi.Config;
using LoyaltyApi.Models;
using LoyaltyApi.RequestModels;
using LoyaltyApi.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.Google;


namespace LoyaltyApi.Controllers;

/// <summary>
/// Controller to handle Google OAuth2 authentication.
/// </summary>
[ApiController]
[Route("api/oauth2")]
public class GoogleOAuth2Controller(OAuth2Service oauth2Service,
ILogger<GoogleOAuth2Controller> logger,
IUserService userService,
ITokenService tokenService,
IOptions<JwtOptions> jwtOptions) : ControllerBase
{
    [HttpGet("externallogin")]
    public async Task<IActionResult> ExternalLogin()
    {
        var properties = new AuthenticationProperties { RedirectUri = Url.Action(nameof(GoogleCallback)) };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Initiates the Google sign-in process.
    /// </summary>
    /// <param name="restaurantId">The ID of the restaurant initiating the sign-in.</param>
    /// <returns>
    /// A Challenge result to redirect the user to Google's OAuth2 login page.
    /// </returns>
    /// <response code="302">Redirects to Google's OAuth2 login page.</response>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/oauth2/signin-google?restaurantId=1
    ///
    /// </remarks>
     [HttpPost("signin-google")]
    public async Task<ActionResult> SignInWithGoogle([FromBody] OAuth2Body body)
    {
        var user = await oauth2Service.HandleGoogleSignIn(body.AccessToken);
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

        string refreshToken = await tokenService.GenerateOrGetRefreshTokenAsync(existingUser.Id, existingUser.RestaurantId, Role.User);
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

    [HttpGet("google-callback")]
    public async Task<ActionResult> GoogleCallback()
    {
        // var user = await oauth2Service.HandleGoogleSignIn(body.AccessToken);
        // var existingUser = await userService.GetUserByEmailAsync(user.Email, body.RestaurantId);
        // if (existingUser is null)
        // {
        //     var registerBody = new RegisterRequestBody()
        //     {
        //         Email = user.Email,
        //         Name = user.Name,
        //         RestaurantId = body.RestaurantId
        //     };
        //     existingUser = await userService.CreateUserAsync(registerBody) ?? throw new HttpRequestException("Failed to create user.");
        // }
        // string accessToken = await tokenService.GenerateAccessTokenAsync(existingUser.Id, existingUser.RestaurantId, Role.User);
        // string refreshToken = await tokenService.GenerateRefreshTokenAsync(existingUser.Id, existingUser.RestaurantId, Role.User);
        // HttpContext.Response.Cookies.Append("refreshToken", refreshToken, jwtOptions.Value.JwtCookieOptions);
        // return Ok(new
        // {
        //     success = true,
        //     message = "Login successful",
        //     data = new
        //     {
        //         accessToken,
        //         user = existingUser
        //     }
        // });
        return Ok(new { success = true, message = "Login successful, STUB", data = new { } });
    }
}