using System.Security.Claims;
using LoyaltyApi.RequestModels;
using LoyaltyApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class RestaurantController(
        IRestaurantService restaurantService,
        ILogger<RestaurantController> logger) : Controller
    {
        /// <summary>
        /// Creates a new restaurant.
        /// </summary>
        ///
        /// <param name="createRestaurantRequest">The request model containing the restaurant data.</param>
        /// <response code="201">The restaurant was created successfully.</response>
        /// <response code="500">If any other exception occurs.</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/admin/restaurants
        ///     {
        ///         "restaurantId": 600,
        ///         "creditPointsBuyingRate": 0.5,
        ///         "creditPointsSellingRate": 1.0,
        ///         "loyaltyPointsBuyingRate": 0,
        ///         "loyaltyPointsSellingRate": 0,
        ///         "creditPointsLifeTime": 1000,
        ///         "loyaltyPointsLifeTime": 0,
        ///         "voucherLifeTime": 86400000
        ///     }
        ///
        /// Sample response:
        ///
        ///     201 Created
        ///     {
        ///         "success": true,
        ///         "message": "Restaurant created"
        ///     }
        ///
        /// Authorization header with Admin Options.
        /// </remarks>
        /// <returns>Action result containing the response.</returns>
        [HttpPost]
        [Route("admin/restaurants")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateRestaurant(
            [FromBody] CreateRestaurantRequestModel createRestaurantRequest)
        {
            logger.LogInformation("Create restaurant request for restaurant with id {id}",
                createRestaurantRequest.RestaurantId);

            await restaurantService.CreateRestaurant(createRestaurantRequest);
            return StatusCode(StatusCodes.Status201Created, new { success = true, message = "Restaurant created" });
        }

        /// <summary>
        /// Retrieves a specific restaurant by its ID.
        /// </summary>
        /// <param name="id">The ID of the restaurant to retrieve.</param>
        /// <returns> The restaurant with the specified ID.</returns>
        /// <response code="200">The restaurant was retrieved successfully.</response>
        /// <response code="404">If the restaurant is not found.</response>
        /// <response code="500">If any other exception occurs.</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/admin/restaurants/1
        ///
        /// Sample response:
        ///
        ///     200 OK
        ///     {
        ///         "success": true,
        ///         "message": "Restaurant retrieved successfully.",
        ///         "data": {
        ///             "restaurant": {
        ///                 "restaurantId": 1,
        ///                 "creditPointsBuyingRate": 0.5,
        ///                 "creditPointsSellingRate": 1.0,
        ///                 "loyaltyPointsBuyingRate": 0,
        ///                 "loyaltyPointsSellingRate": 0,
        ///                 "creditPointsLifeTime": 1000,
        ///                 "loyaltyPointsLifeTime": 0,
        ///                 "voucherLifeTime": 86400000
        ///             }
        ///         }
        ///     }
        /// 
        /// **Admin Only Endpoint**
        /// Authorization header with Admin Options.
        /// </remarks>
        [HttpGet]
        [Route("admin/restaurants/{id}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetRestaurant([FromRoute] int id)
        {
            logger.LogInformation("Get restaurant request for restaurant with id {id}", id);

            var result = await restaurantService.GetRestaurantById(id);
            if (result == null) return NotFound(new { success = false, message = "Restaurant not found" });
            return Ok(new
            {
                success = true,
                message = "Restaurant retrieved successfully.",
                data = new { restaurant = result }
            });
        }

        /// <summary>
        /// Updates a specific restaurant by its ID.
        /// </summary>
        /// <param name="id">The ID of the restaurant to update.</param>
        /// <param name="updateRestaurant">The updated restaurant data.</param>
        /// <returns> The updated restaurant.</returns>
        /// <response code="200">The restaurant was updated successfully.</response>
        /// <response code="404">If the restaurant is not found.</response>
        /// <response code="500">If any other exception occurs.</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /api/admin/restaurants/1
        ///     {
        ///         "creditPointsBuyingRate": 0.5,
        ///         "creditPointsSellingRate": 1.0,
        ///         "creditPointsLifeTime": 1000,
        ///         "voucherLifeTime": 86400000
        ///     }
        ///
        /// Sample response:
        ///
        ///     200 OK
        ///     {
        ///         "success": true,
        ///         "message": "Restaurant updated successfully."
        ///     }
        /// 
        /// Authorization header with Admin Options.
        /// </remarks>
        [HttpPut]
        [Route("admin/restaurants/{id}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateRestaurant([FromRoute] int id,
            [FromBody] RestaurantCreditPointsRequestModel updateRestaurant)
        {
            logger.LogInformation("Update restaurant request for restaurant with id {id}", id);

            await restaurantService.UpdateRestaurantInfo(id, updateRestaurant);
            return Ok(new { success = true, message = "Restaurant Updated" });
        }

        /// <summary>
        /// Retrieves the restaurant information for the authenticated user.
        /// </summary>
        /// <returns>The restaurant information for the current user's restaurant.</returns>
        /// <response code="200">The restaurant was retrieved successfully.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="404">If the restaurant is not found.</response>
        /// <response code="500">If any other exception occurs.</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/restaurants/me
        ///
        /// Sample response:
        ///
        ///     200 OK
        ///     {
        ///         "success": true,
        ///         "message": "Restaurant retrieved successfully.",
        ///         "data": {
        ///             "restaurant": {
        ///                 "restaurantId": 1,
        ///                 "creditPointsBuyingRate": 0.5,
        ///                 "creditPointsSellingRate": 1.0,
        ///                 "loyaltyPointsBuyingRate": 0,
        ///                 "loyaltyPointsSellingRate": 0,
        ///                 "creditPointsLifeTime": 1000,
        ///                 "loyaltyPointsLifeTime": 0,
        ///                 "voucherLifeTime": 86400000
        ///             }
        ///         }
        ///     }
        /// 
        /// Authorization header with Admin Options..
        /// </remarks>
        [HttpGet]
        [Route("restaurants/me")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult> GetRestaurantByJwtToken()
        {
            logger.LogInformation("Get restaurant request for restaurant with id {id}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            string restaurantClaim = User.FindFirst("restaurantId")?.Value ?? throw new UnauthorizedAccessException("Restaurant id not found in token");
            _ = int.TryParse(restaurantClaim, out var restaurantId);
            var result = await restaurantService.GetRestaurantById(restaurantId);
            if (result == null) return NotFound(new { success = false, message = "Restaurant not found" });
            return Ok(new
            {
                success = true,
                message = "Restaurant retrieved successfully.",
                data = new { restaurant = result }
            });
        }

    }
}