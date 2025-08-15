using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace LoyaltyApi.filters;

public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var serviceProvider = context.HttpContext.RequestServices;
        
        // Check FluentValidation for each action argument
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument == null) continue;
            
            var argumentType = argument.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
            
            // Try to get the validator from DI container
            var validator = serviceProvider.GetService(validatorType) as IValidator;
            
            if (validator != null)
            {
                // Create validation context and validate
                var validationContext = new ValidationContext<object>(argument);
                var result = validator.Validate(validationContext);
                
                if (!result.IsValid)
                {
                    var errorMessages = result.Errors.Select(e => e.ErrorMessage).ToList();
                    
                    context.Result = new BadRequestObjectResult(new
                    {
                        message = errorMessages,
                        success = false
                    });
                    return;
                }
            }
        }
        
        // Fallback: Check ModelState for any other validation errors
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Any() == true)
                .SelectMany(x => x.Value!.Errors.Select(error => error.ErrorMessage))
                .ToList();

            context.Result = new BadRequestObjectResult(new
            {
                message = errors,
                success = false
            });
        }
    }
}