using LoyaltyApi.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace LoyaltyApi.Middlewares;

public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;

    public GlobalExceptionHandler(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(httpContext, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
    {
        object errorResponse;
        int statusCode;

        if (exception is UnauthorizedAccessException)
        {
            errorResponse = new { success = false, message = exception.Message };
            statusCode = StatusCodes.Status401Unauthorized;
        }
        else if (exception is EmailAlreadyConfirmedException)
        {
            errorResponse = new { success = false, message = exception.Message };
            statusCode = StatusCodes.Status409Conflict;
        }
        else if (exception is SecurityTokenMalformedException)
        {
            errorResponse = new { success = false, message = exception.Message };
            statusCode = StatusCodes.Status400BadRequest;
        }
        else if (exception is MinimumTransactionAmountNotReachedException)
        {
            errorResponse = new { success = false, message = exception.Message };
            statusCode = StatusCodes.Status422UnprocessableEntity;
        }
        else if (exception is PointsNotEnoughException)
        {
            errorResponse = new { success = false, message = exception.Message };
            statusCode = StatusCodes.Status409Conflict;
        }
        else if (exception is DuplicateReceiptException)
        {
            errorResponse = new { success = false, message = exception.Message };
            statusCode = StatusCodes.Status409Conflict;
        }
        else if (exception is HttpRequestException)
        {
            errorResponse = new { success = false, message = "An error occurred while processing your request. Please try again later." };
            statusCode = StatusCodes.Status500InternalServerError;
        }
        else if (exception is NullReferenceException)
        {
            errorResponse = new { success = false, message = "A required resource was not found." };
            statusCode = StatusCodes.Status404NotFound;
        }
        else if (exception is SecurityTokenException)
        {
            errorResponse = new { success = false, message = "Invalid token." };
            statusCode = StatusCodes.Status401Unauthorized;
        }
        else
        {
            errorResponse = new { success = false, message = exception.Message };
            statusCode = StatusCodes.Status500InternalServerError;
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(errorResponse);
    }
}