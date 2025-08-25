using LoyaltyApi.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace LoyaltyApi.filters
{
    /// <summary>
    /// Action filter attribute to validate API key for specific endpoints.
    /// Usage: [ApiKeyValidator] on controller or action methods.
    /// </summary>
    public class ApiKeyValidatorAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var apiKeyOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<ApiKey>>();
            
            _ = context.HttpContext.Request.Headers.TryGetValue("X-ApiKey", out var extractedApiKey);
            
            if (!apiKeyOptions.Value.Key.Equals(extractedApiKey))
            {
                context.Result = new UnauthorizedObjectResult(new { });
                return;
            }
            
            base.OnActionExecuting(context);
        }
    }
}
