using LoyaltyApi.Config;
using LoyaltyApi.Exceptions;
using LoyaltyApi.Models;
using LoyaltyApi.RequestModels;
using LoyaltyApi.Services;
using LoyaltyApi.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LoyaltyApi.Controllers;

/// <summary>
/// Controller for handling authentication-related operations.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController(
    ITokenService tokenService,
    IOptions<JwtOptions> jwtOptions,
    IUserService userService,
    IOptions<AdminOptions> adminOptions,
    IPasswordService passwordService,
    ILogger<AuthController> logger,
    TokenUtility tokenUtility) : ControllerBase
{
    /// <summary>
    /// Authenticates a user and generates a JWT token.
    /// </summary>
    /// <param name="loginBody">The login request body containing email, phone number, password, and restaurant ID.</param>
    /// <returns>
    /// An Ok result containing the generated JWT token, or a BadRequest result if the email or phone number is not provided,
    /// or an Unauthorized result if the credentials are invalid, or an InternalServerError result if any other exception occurs.
    /// </returns>
    /// <response code="200">Returns the generated JWT token.</response>
    /// <response code="400">If the email or phone number is not provided.</response>
    /// <response code="401">If the credentials are invalid.</response>
    /// <response code="500">If any other exception occurs.</response>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/auth/login
    ///     {
    ///        "email": "user@example.com",
    ///        "phoneNumber": "1234567890",
    ///        "password": "password123",
    ///        "restaurantId": 1
    ///     }
    ///
    /// Sample response:
    ///
    ///     200 OK
    ///     {
    ///        "success": true,
    ///        "message": "Login successful",
    ///           "data": {
    ///            "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    ///             "user": {
    ///                  "id": "1",
    ///                  "email": "email@example.com",
    ///                  "phoneNumber": "9876543210",
    ///                  "restaurantId": "1",
    ///                  "name": "John Doe",
    ///              }
    ///         }
    ///     }
    /// </remarks>
    [HttpPost]
    [Route("login")]
    public async Task<ActionResult> Login([FromBody] LoginRequestBody loginBody)
    {
        logger.LogInformation("Login request for restaurant {RestaurantId}", loginBody.RestaurantId);
        if (loginBody.Email == null && loginBody.PhoneNumber == null)
            return BadRequest(new { success = false, message = "Email or Phone number is required" });

        if (loginBody.Email == adminOptions.Value.Username && loginBody.Password == adminOptions.Value.Password)
        {
            logger.LogInformation("Admin login request for restaurant {RestaurantId}", loginBody.RestaurantId);
            string accessTokenAdmin = await tokenService.GenerateAccessTokenAsync(0, loginBody.RestaurantId, Role.Admin);
            return Ok(new
            {
                success = true,
                message = "Admin login successful",
                data = new { accessToken = accessTokenAdmin }
            });
        }

        User? user = loginBody.Email != null
            ? await userService.GetUserByEmailAsync(loginBody.Email, loginBody.RestaurantId)
            : await userService.GetUserByPhonenumberAsync(
                loginBody.PhoneNumber ?? throw new ArgumentException("Phone number is required"),
                loginBody.RestaurantId);

        if (user == null)
            return Unauthorized(new { success = false, message = "no user found" });

        Password? password =
            await passwordService.GetAndValidatePasswordAsync(user.Id, user.RestaurantId, loginBody.Password);

        logger.LogInformation($"email confirmation is {password.ConfirmedEmail}");
        
        if (password is null)
            return Unauthorized(new { success = false, message = "Invalid password." });

        if (password.ConfirmedEmail == false)
            return Unauthorized(new { success = false, message = "Email not confirmed." });

        var accessToken = await tokenService.GenerateAccessTokenAsync(user.Id, loginBody.RestaurantId, Role.User);

        // Cookie-only approach: Always generate fresh refresh token
        // Clear any existing refresh token cookie first
        HttpContext.Response.Cookies.Delete("refreshToken");

        // Generate new refresh token (no database storage)
        string refreshToken = tokenService.GenerateRefreshToken(user.Id, loginBody.RestaurantId, Role.User);

        // Set secure cookie with refresh token
        HttpContext.Response.Cookies.Append("refreshToken", refreshToken, jwtOptions.Value.JwtCookieOptions);

        return Ok(new
        {
            success = true,
            message = "Login successful",
            data = new
            {
                accessToken,
                user
            }
        });
    }

    /// <summary>
    /// Logs out a user by clearing the refresh token cookie.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///     POST /api/auth/logout
    /// </remarks>
    /// <returns>A confirmation message.</returns>
    /// <response code="200">If the logout is successful.</response>
    [HttpPost]
    [Route("logout")]
    public ActionResult Logout()
    {
        logger.LogInformation("User logout request");

        // Clear refresh token cookie
        HttpContext.Response.Cookies.Delete("refreshToken");

        // Optionally, you can also expire it explicitly
        HttpContext.Response.Cookies.Append("refreshToken", "", new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddDays(-1),
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        });

        return Ok(new
        {
            success = true,
            message = "Logout successful"
        });
    }

    /// <summary>
    /// Confirms a user's email based on the provided token.
    /// </summary>
    /// <param name="token">The token used to confirm the email.</param>
    /// <returns>
    /// An Ok result if the email is confirmed successfully, or an Unauthorized result if the token is not provided,
    /// or an InternalServerError result if any other exception occurs.
    /// </returns>
    /// <response code="200">If the email is confirmed successfully.</response>
    /// <response code="401">If the token is not provided.</response>
    /// <response code="500">If any other exception occurs.</response>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/auth/confirm-email/{token}
    ///
    /// Sample response:
    ///
    ///     200 OK
    ///     {
    ///         "success": true,
    ///         "message": "Email confirmed successfully."
    ///         "data": {
    ///             "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    ///             "user": {
    ///                  "id": "1",
    ///                  "email": "email@example.com",
    ///                  "phoneNumber": "9876543210",
    ///                  "restaurantId": "1",
    ///                  "name": "John Doe",
    ///              }
    ///         }
    ///     }
    /// </remarks>
    [HttpPut]
    [Route("confirm-email/{token}")]
    public async Task<ActionResult> ConfirmEmail(string token)
    {
        logger.LogInformation("Confirm email request for token {Token}", token);
        await passwordService.ConfirmEmail(token);
        Token confirmEmailToken = tokenUtility.ReadToken(token);
        User? user = await userService.GetUserByIdAsync(confirmEmailToken.CustomerId, confirmEmailToken.RestaurantId) ?? throw new NullReferenceException("User not found");
        string accessToken = await tokenService.GenerateAccessTokenAsync(user.Id, user.RestaurantId, Role.User);

        return Ok(new
        {
            success = true,
            message = "Email confirmed successfully.",
            data = new
            {
                user,
                accessToken
            }
        });
    }
}