using FluentValidation;
using LoyaltyApi.RequestModels;

namespace LoyaltyApi.Validators;

public class LoginRequestBodyValidator : AbstractValidator<LoginRequestBody>
{
    public LoginRequestBodyValidator()
    {  
        // Restaurant ID validation
        RuleFor(x => x.RestaurantId)
            .GreaterThan(0).WithMessage("Restaurant Id must be greater than 0.");
        
        // At least one of Email or PhoneNumber must be provided
        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.Email) || !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Either Email or PhoneNumber must be provided.");
        
        // If Email is provided, validate it
        When(x => !string.IsNullOrEmpty(x.Email), () => {
            RuleFor(x => x.Email!)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");
        });
        
    }
}
