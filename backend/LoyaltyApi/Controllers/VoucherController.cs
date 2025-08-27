using System.Security.Claims;
using LoyaltyApi.Exceptions;
using LoyaltyApi.filters;
using LoyaltyApi.Models;
using LoyaltyApi.RequestModels;
using LoyaltyApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyApi.Controllers;

/// <summary>
///  Controller for managing voucher-related operations.
/// </summary>
[ApiController]
[Route("api")]
public class VoucherController(
    IVoucherService voucherService,
    ICreditPointsTransactionService pointsTransactionService,
    ILogger<VoucherController> logger) : ControllerBase
{
    /// <summary>
    /// Create a new voucher.
    /// </summary>
    /// <param name="voucherRequest">The voucher request details.</param>
    /// <returns>The created voucher.</returns>
    /// <response code="201">Voucher created successfully.</response>
    /// <response code="400">Invalid request body or parameters.</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="500">Server error.</response>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/vouchers
    /// 
    ///     {
    ///          "points": 100
    ///     }
    ///     
    /// Sample response:
    ///
    ///     201 Created
    ///     {
    ///         "success": true,
    ///         "message": "Voucher created",
    ///         "data": {
    ///             "voucher": "ABC123"
    ///         }
    ///     }
    /// 
    /// Authorization header with JWT Bearer token is required.
    /// </remarks>
    [HttpPost]
    [Route("vouchers")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult> CreateVoucher([FromBody] CreateVoucherRequest voucherRequest)
    {
        logger.LogInformation("Creating voucher for customer {CustomerId} and restaurant {RestaurantId}",
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value, User.FindFirst("RestaurantId")?.Value);

        string userClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                            throw new UnauthorizedAccessException("User id not found in token");

        string restaurantClaim = User.FindFirst("restaurantId")?.Value ??
                                    throw new UnauthorizedAccessException("Restaurant id not found in token");

        _ = int.TryParse(userClaim, out var userId);
        _ = int.TryParse(restaurantClaim, out var restaurantId);

        Voucher voucher = await voucherService.CreateVoucherAsync(voucherRequest, userId, restaurantId);

        await pointsTransactionService.SpendPointsAsync(voucher.CustomerId, voucher.RestaurantId,
            voucherRequest.Points);

        return StatusCode(StatusCodes.Status201Created,
            new
            {
                success = true,
                message = "Voucher created",
                data = new
                {
                    voucher
                }
            });
    }

    /// <summary>
    /// Get a voucher by short code.
    /// </summary>
    /// <param name="shortCode">The short code of the voucher.</param>
    /// <returns>The voucher details.</returns>
    /// <response code="200">Voucher retrieved successfully.</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="500">Server error.</response>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/vouchers/ABC123
    ///     
    /// Sample response:
    ///
    ///     200 OK
    ///     {
    ///         "success": true,
    ///         "message": "Voucher retrieved successfully",
    ///         "data": 
    ///         {
    ///             "voucher": 
    ///             {
    ///                 "shortCode": "ABC123",
    ///                 "value": 100
    ///                 "isUsed": false
    ///             }
    ///         }
    ///     }
    ///
    /// shortCode can be provided as query parameter for getting a specific voucher.
    /// Authorization header with JWT Bearer token is required.
    /// </remarks>
    [HttpGet]
    [Route("vouchers/{shortCode}")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult> GetUserVoucherByShortCode(string shortCode)
    {
        logger.LogInformation("Getting voucher {ShortCode} for customer {CustomerId} and restaurant {RestaurantId}",
            shortCode, User.FindFirst(ClaimTypes.NameIdentifier)?.Value, User.FindFirst("RestaurantId")?.Value);

        string userClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                            throw new UnauthorizedAccessException();
        string restaurantClaim = User.FindFirst("restaurantId")?.Value ?? throw new UnauthorizedAccessException();
        _ = int.TryParse(userClaim, out var userId);
        _ = int.TryParse(restaurantClaim, out var restaurantId);
        
        if (userId == 0 || restaurantId == 0)
            throw new UnauthorizedAccessException();

        var voucher = await voucherService.GetVoucherAsync(shortCode);
        var result = new
        {
            voucher.ShortCode,
            voucher.Value,
            voucher.IsUsed
        };
        return Ok(new
        {
            success = true,
            message = "Voucher retrieved successfully",
            data = new { voucher = result }
        });
    }

    /// <summary>
    /// Gets all vouchers for the current user.
    /// </summary>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>The vouchers.</returns>
    /// <response code="200">Vouchers retrieved successfully.</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="500">Server error.</response>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/vouchers?pageNumber=1&amp;pageSize=10
    ///
    /// Sample response:
    ///
    ///     200 OK
    ///     {
    ///         "success": true,
    ///         "message": "Vouchers retrieved successfully",
    ///         "data": {
    ///             "vouchers": [
    ///                 {
    ///                     "shortCode": "ABCD",
    ///                     "value": 100,
    ///                     "isUsed": false
    ///                 },
    ///                 {
    ///                     "shortCode": "EFGH",
    ///                     "value": 200,
    ///                     "isUsed": true
    ///                 }
    ///             ],
    ///             "metadata": {
    ///                 "totalCount": 10,
    ///                 "totalPages": 1,
    ///                 "pageSize": 10,
    ///                 "pageNumber": 1
    ///             }
    ///         }
    ///     }
    ///
    /// Authorization header with JWT Bearer token is required.
    /// </remarks>
    [HttpGet]
    [Route("vouchers")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult> GetVouchersByJwtToken([FromQuery] int? pageNumber, [FromQuery] int? pageSize)
    {
        string userClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                            throw new UnauthorizedAccessException();
        string restaurantClaim = User.FindFirst("restaurantId")?.Value ?? throw new UnauthorizedAccessException();
        _ = int.TryParse(userClaim, out var userId);
        _ = int.TryParse(restaurantClaim, out var restaurantId);
        var paginationResult =
            await voucherService.GetUserVouchersAsync(userId, restaurantId, pageNumber ?? 1, pageSize ?? 10);
        var vouchers = paginationResult.Vouchers;
        var vouchersResult = vouchers.Select(voucher => new
        {
            voucher.ShortCode,
            voucher.DateOfCreation,
            voucher.Value,
            voucher.IsUsed
        });
        var paginationMetadata = paginationResult.PaginationMetadata;
        return Ok(new
        {
            success = true,
            message = "Vouchers retrieved successfully",
            data = new { vouchers = vouchersResult },
            metadata = paginationMetadata
        });
    }

    // /// <summary>
    // /// Get a voucher by short code for admin purposes.
    // /// </summary>
    // /// <param name="shortCode">The short code of the voucher.</param>
    // /// <param name="customerId">The customer ID associated with the voucher.</param>
    // /// <param name="restaurantId">The restaurant ID associated with the voucher.</param>
    // /// <returns>The long code of the voucher.</returns>
    // /// <response code="200">Voucher retrieved successfully.</response>
    // /// <response code="401">Unauthorized access.</response>
    // /// <response code="500">Server error.</response>
    // /// <remarks>
    // /// Sample request:
    // ///
    // ///     GET admin/restaurants/600/users/7/vouchers/ABCDE
    // ///
    // /// Sample response:
    // ///
    // ///     200 OK
    // ///     {
    // ///         "success": true,
    // ///         "message": "Voucher retrieved successfully",
    // ///         "data": {
    // ///             "voucher":
    // ///             {
    // ///                 "shortCode": "ABCDE",
    // ///                 "longCode": "1234567890",
    // ///                 "value": 100,
    // ///                 "isUsed": false
    // ///             }
    // ///         }
    // ///     }
    // ///
    // /// Authorization header with Admin Options.
    // /// </remarks>
    // [HttpGet]
    // [Route("admin/vouchers/{shortCode}/restaurants/{restaurantId}/users/{customerId}")]
    // // [Authorize(Roles = "Admin")]
    // public async Task<ActionResult> GetVoucherLongCode(int restaurantId, int customerId, string shortCode)
    // {
    //     logger.LogInformation("Getting voucher {ShortCode} for customer {CustomerId} and restaurant {RestaurantId}",
    //         shortCode, customerId, restaurantId);

    //     Voucher voucher = await voucherService.GetVoucherAsync(customerId, restaurantId, shortCode);
    //     return Ok(new
    //     {
    //         success = true,
    //         message = "Voucher retrieved successfully",
    //         data = new
    //         {
    //             voucher = new
    //             {
    //                 voucher.ShortCode,
    //                 voucher.LongCode,
    //                 voucher.Value,
    //                 voucher.IsUsed
    //             }
    //         }
    //     });
    // }


    /// <summary>
    /// Sets a voucher iUsed.
    /// </summary>
    /// <param name="shortCode">The short code of the voucher.</param>
    /// <param name="requestModel">The request model containing the value to set the voucher to used to.</param>
    /// <returns>An ActionResult indicating the result of the operation.</returns>
    /// <response code="200">If the voucher is set to used successfully.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="500">If any other exception occurs.</response>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /api/vouchers/ABCDE
    ///     {
    ///         "isUsed": true
    ///     }
    ///
    /// Sample response:
    ///     200 OK
    ///     {
    ///         "success": true,
    ///         "message": "Voucher isUsed set to true",
    ///         "data": {
    ///             "voucher": {
    ///                 "shortCode": "ABCDE",
    ///                 "longCode": "1234567890",
    ///                 "value": 100,
    ///                 "isUsed": true
    ///             }
    ///         }
    ///     }
    ///
    /// Authorization header with Admin Options.
    /// </remarks>
    [HttpPut]
    [Route("admin/vouchers/{shortCode}")]
    [ApiKeyValidator]
    // [Authorize(Roles = "Admin")]
    public async Task<ActionResult> SetIsUsed([FromRoute] string shortCode)
    {
        logger.LogInformation("Setting voucher {ShortCode} to used", shortCode);

        Voucher voucher = await voucherService.SetIsUsedAsync(shortCode);
        return Ok(new
        {
            success = true,
            message = $"Voucher isUsed set to {true}",
            data = new
            {
                voucher = new
                {
                    voucher.ShortCode,
                    voucher.Value,
                    voucher.IsUsed
                }
            }
        });
    }

    /// <summary>
    /// Gets all vouchers for a customer with a specific restaurant ID.
    /// </summary>
    /// <param name="restaurantId">The restaurant ID.</param>
    /// <param name="customerId">The customer ID.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>The vouchers.</returns>
    /// <response code="200">Vouchers retrieved successfully.</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="500">Server error.</response>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/admin/vouchers/restaurants/1/users/2?pageNumber=1&amp;pageSize=10
    ///
    /// Sample response:
    ///
    ///     200 OK
    ///     {
    ///         "success": true,
    ///         "message": "Vouchers retrieved successfully",
    ///         "data": {
    ///             "vouchers": [
    ///                 {
    ///                     "shortCode": "ABCD",
    ///                     "value": 100,
    ///                     "isUsed": false
    ///                 },
    ///                 {
    ///                     "shortCode": "EFGH",
    ///                     "value": 200,
    ///                     "isUsed": true
    ///                 }
    ///             ],
    ///             "metadata": {
    ///                 "totalCount": 25,
    ///                 "totalPages": 3,
    ///                 "pageSize": 2,
    ///                 "pageNumber": 1
    ///             }
    ///         }
    ///     }
    ///
    /// Authorization header with Admin Options.
    /// </remarks>
    [HttpGet]
    [Route("admin/vouchers/restaurants/{restaurantId}/users/{customerId}")]
    // [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GetUserVouchersByRestaurantIdAndCustomerId([FromRoute] int restaurantId, [FromRoute] int customerId, [FromQuery] int pageNumber, [FromQuery] int pageSize)
    {
        logger.LogInformation("Getting vouchers for customer {CustomerId}", customerId);

        PagedVouchersResponse paginationResult = await voucherService.GetUserVouchersAsync(customerId, restaurantId, pageNumber, pageSize);
        var vouchers = paginationResult.Vouchers.Select(v => new
        {
            v.ShortCode,
            v.Value,
            v.IsUsed
        }).ToList();
        var metadata = paginationResult.PaginationMetadata;
        return Ok(new
        {
            success = true,
            message = "Vouchers retrieved successfully",
            data = new
            {
                vouchers
            },
            metadata
        });
    }
}