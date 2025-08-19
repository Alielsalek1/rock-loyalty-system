using System.Security.Claims;
using FluentEmail.Core;
using LoyaltyApi.Config;
using LoyaltyApi.Models;
using LoyaltyApi.RequestModels;
using LoyaltyApi.Services;
using LoyaltyApi.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sprache;

namespace LoyaltyApi.Controllers;

/// <summary>
/// Controller for managing user-related operations.
/// </summary>
[ApiController]
[Route("api/users")]
public class UsersController(
    IUserService userService,
    ITokenService tokenService,
    ICreditPointsTransactionService pointsTransactionService,
    ILogger<UsersController> logger,
    IOptions<FrontendOptions> frontendOptions,
    IPasswordService passwordService,
    EmailService emailService) : ControllerBase
{
    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="requestBody">The request body containing user registration details.</param>
    /// <returns>The created user.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/users
    ///     {
    ///         "name": "John Doe",
    ///         "email": "user@example.com",
    ///         "password": "password123",
    ///         "phoneNumber": "1234567890",
    ///         "restaurantId": "1"
    ///     }
    /// 
    /// Sample response:
    ///
    ///     201 Created
    ///     {
    ///         "success": true,
    ///         "message": "User found",
    ///         "data": {
    ///             "user": {
    ///                  "id": "1",
    ///                  "email": "user@example.com",
    ///                  "phoneNumber": "1234567890",
    ///                  "restaurantId": "1",
    ///                  "name": "John Doe",
    ///              }
    ///         }
    ///     }
    ///
    /// Authorization header with JWT Bearer token is required.
    /// </remarks>
    /// <response code="201">If the user is created successfully.</response>
    /// <response code="400">If the request body is invalid.</response>
    /// <response code="500">If any other exception occurs.</response>
    [HttpPost]
    [Route("")]
    public async Task<ActionResult> CreateUser([FromBody] RegisterRequestBody requestBody)
    {
        logger.LogInformation("Create user request for restaurant with id {id}", requestBody.RestaurantId);

        User? existingUser =
            await userService.GetUserByEmailAsync(requestBody.Email, requestBody.RestaurantId) ??
            await userService.GetUserByPhonenumberAsync(requestBody.PhoneNumber ?? "0",
                requestBody.RestaurantId);

        if (requestBody.Password is null)
            return BadRequest(new { success = false, message = "Password is required" });

        if (existingUser is not null)
            return BadRequest(new { success = false, message = "User already exists" });

        User? user = await userService.CreateUserAsync(requestBody);

        var confirmToken = await tokenService.GenerateConfirmEmailTokenAsync(user.Id, user.RestaurantId);
        await emailService.SendEmailAsync(user.Email, $"Email Confirmation for Loyalty System",
            $"Welcome to Loyalty System. Please Confirm your email by clicking on the following link: {frontendOptions.Value.BaseUrl}/{user.RestaurantId}/auth/confirm-email/" +
            confirmToken, "Rock Loyalty System");

        if (user == null) return StatusCode(500, new { success = false, message = "User creation failed" });

        return StatusCode(StatusCodes.Status201Created,
            new { success = true, message = "User created", data = new { user } });
    }

    /// <summary>
    /// Retrieves a user by their ID.
    /// </summary>
    /// <returns> The user with the specified ID.</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/users/1/restaurants/600
    /// 
    /// 
    /// Sample response:
    /// 
    ///     200 OK
    ///     {
    ///         "success": true,
    ///         "message": "User found",
    ///         "data": {
    ///             "user": {
    ///                  "id": "1",
    ///                  "email": "user@example.com",
    ///                  "phoneNumber": "1234567890",
    ///                  "restaurantId": "1",
    ///                  "name": "John Doe",
    ///              }
    ///            "points": 100
    ///         }
    ///     }
    /// 
    /// Authorization header with JWT Bearer token is required.
    /// </remarks>
    /// <response code="200">If the user is found successfully.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="404">If the user is not found.</response>
    /// <response code="500">If any other exception occurs.</response>
    [HttpGet]
    [Route("{userId}/restaurants/{restaurantId}")]
    // [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GetUserById(int userId, int restaurantId)
    {
        logger.LogInformation("Get user request for user with id {id} for restaurant with id {restaurantId}",
            userId, restaurantId);

        User? user = await userService.GetUserByIdAsync(userId, restaurantId);

        if (user == null) return NotFound(new { success = false, message = "User not found" });

        int points = await pointsTransactionService.GetCustomerPointsAsync(user.Id, user.RestaurantId);
        return Ok(new
        {
            success = true,
            message = "User found",
            data = new
            {
                user,
                points
            }
        });
    }

    /// <summary>
    /// Retrieves the details of the current user by their JWT Bearer token.
    /// </summary>
    /// <returns> The details of the current user.</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/users/me
    ///
    /// 
    /// Sample response:
    ///
    ///     200 OK
    ///     {
    ///         "success": true,
    ///         "message": " User retrieved successfully",
    ///         "data": {
    ///             "user": {
    ///                  "id": "1",
    ///                  "email": "user@example.com",
    ///                  "phoneNumber": "1234567890",
    ///                  "restaurantId": "1",
    ///                  "name": "John Doe",
    ///              }
    ///             "points": 100
    ///         }
    ///     }
    ///
    /// Authorization header with JWT Bearer token is required.
    /// </remarks>
    /// <response code="200">If the user is found successfully.</response>
    /// <response code="404">If the user is not found.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="500">If any other exception occurs.</response>
    [HttpGet]
    [Route("me")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult> GetUserByJwtToken()
    {
        logger.LogInformation("Get user request for user with id {id}",
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        string userClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                            throw new UnauthorizedAccessException("User id not found in token");
        string restaurantClaim = User.FindFirst("restaurantId")?.Value ??
                                    throw new UnauthorizedAccessException("Restaurant id not found in token");
        _ = int.TryParse(userClaim, out var userId);
        _ = int.TryParse(restaurantClaim, out var restaurantId);
        User? user = await userService.GetUserByIdAsync(userId, restaurantId);
        if (user == null) return NotFound(new { success = false, message = "User not found" });
        int points = await pointsTransactionService.GetCustomerPointsAsync(user.Id, user.RestaurantId);
        return Ok(new
        {
            success = true,
            message = "User retrieved successfully",
            data = new
            {
                user,
                points,
            }
        });
    }

    /// <summary>
    /// Updates a user by their ID.
    /// </summary>
    /// <returns> The updated user. </returns>
    /// <remarks>
    ///
    /// Sample request:
    ///
    ///     PUT /api/users/1/restaurant/600
    ///     {
    ///         "phoneNumber": "9876543210",
    ///         "email": "newemail@example.com",
    ///         "name": "Jane Doe"
    ///     }
    /// 
    /// Sample response:
    ///
    ///     200 OK
    ///     {
    ///         "success": true,
    ///         "message": "User updated",
    ///         "data": {
    ///             "user": {
    ///                  "id": "1",
    ///                  "email": "newemail@example.com",
    ///                  "phoneNumber": "9876543210",
    ///                  "restaurantId": "600",
    ///                  "name": "Jane Doe",
    ///              }
    ///         }
    ///     }
    ///
    /// Authorization header with JWT Bearer token is required.
    /// </remarks>
    /// <response code="200">If the user is found successfully.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="404">If the user is not found.</response>
    /// <response code="500">If any other exception occurs.</response>
    [HttpPut]
    [Route("{userId}/restaurant/{restaurantId}")]
    // [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateUserById(int userId, int restaurantId, [FromBody] UpdateUserRequestModel requestBody)
    {
        logger.LogInformation("Update user request for user with id {id} and restaurant {restaurantId}", userId, restaurantId);

        User? existingUser = await userService.GetUserByIdAsync(userId, restaurantId) ?? throw new Exception("User is not found");
        if (existingUser.Email != requestBody.Email) await passwordService.UnConfirmEmail(userId, restaurantId);

        User user = await userService.UpdateUserAsync(requestBody, userId, restaurantId);
        if (user == null) return NotFound(new { success = false, message = "User not found" });

        return Ok(new
        {
            success = true,
            message = "User updated",
            data = new { user }
        });
    }
    [HttpPut]
    [Route("")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult> UpdateUserByJwt([FromBody] UpdateUserRequestModel requestBody)
    {
        logger.LogInformation("Update user request for user with id {id} and restaurant {restaurantId}",
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            User.FindFirst("restaurantId")?.Value);

        string userClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User id not found in token");
        string restaurantClaim = User.FindFirst("restaurantId")?.Value ?? throw new UnauthorizedAccessException("Restaurant id not found in token");
        _ = int.TryParse(userClaim, out var userId);
        _ = int.TryParse(restaurantClaim, out var restaurantId);
        User? existingUser = await userService.GetUserByIdAsync(userId, restaurantId) ?? throw new Exception("User is not found");
        if (existingUser.Email != requestBody.Email) await passwordService.UnConfirmEmail(userId, restaurantId);
        User user = await userService.UpdateUserAsync(requestBody, userId, restaurantId);
        if (user == null) return NotFound(new { success = false, message = "User not found" });
        return Ok(new
        {
            success = true,
            message = "User updated",
            data = new { user }
        });
    }

}
