using System.Security.Claims;
using LoyaltyApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyApi.Controllers;

/// <summary>
/// Controller for handling credit points.
/// </summary>
[ApiController]
[Route("api/credit-points")]
public class CreditPointsController(
    ICreditPointsTransactionService pointsTransactionService,
    ILogger<CreditPointsController> logger) : ControllerBase
{
    /// <summary>
    /// Retrieves the credit points for the authenticated user.
    /// </summary>
    /// <returns>Returns the credit points of the user.</returns>
    /// <response code="200">Returns the credit points of the user.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="500">If there is an internal server error.</response>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/credit-points
    ///
    /// Sample response:
    ///
    ///     200 OK
    ///     {
    ///         "success": true,
    ///         "data": {
    ///             "points": 100
    ///         },
    ///         "message": "Points retrieved successfully"
    ///     }
    ///
    /// Authorization header with JWT Bearer token is required.
    /// </remarks>
    [HttpGet]
    [Route("")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult> GetPoints()
    {
        string userClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException();
        string restaurantClaim = User.FindFirst("restaurantId")?.Value ?? throw new UnauthorizedAccessException();
        _ = int.TryParse(userClaim, out var userId);
        _ = int.TryParse(restaurantClaim, out var restaurantId);
        logger.LogInformation("Get points request");
        var points = await pointsTransactionService.GetCustomerPointsAsync(userId, restaurantId);
        return Ok(new { success = true, data = new { points }, message = "Points retrieved successfully" });
    }
}