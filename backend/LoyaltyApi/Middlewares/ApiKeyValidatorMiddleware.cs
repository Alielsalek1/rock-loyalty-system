using Azure.Core;
using LoyaltyApi.Config;
using Microsoft.Extensions.Options;

namespace LoyaltyApi.Middlewares
{
    public class ApiKeyValidatorMiddleware(RequestDelegate next,
    IOptions<ApiKey> apiKeyOptions)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            _ = context.Request.Headers.TryGetValue("X-ApiKey",
                out var extractedApiKey);
            if (!apiKeyOptions.Value.Key.Equals(extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "Api key is not provided or invalid"
                });
                return;
            }
            await next(context);
        }
    }
}