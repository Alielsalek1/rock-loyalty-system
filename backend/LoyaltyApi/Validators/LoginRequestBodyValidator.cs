using FluentValidation;
using LoyaltyApi.RequestModels;

namespace LoyaltyApi.Validators;

public class LoginRequestBodyValidator : AbstractValidator<LoginRequestBody>
{
    public LoginRequestBodyValidator()
    {
        // Password validation
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
        
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
        
        // If PhoneNumber is provided, validate it
        When(x => !string.IsNullOrEmpty(x.PhoneNumber), () => {
            RuleFor(x => x.PhoneNumber!)
                .Matches(@"^\+?[1-9]\d{1,14}$")
                .WithMessage("Phone number must be a valid format.");
        });
    }
}
