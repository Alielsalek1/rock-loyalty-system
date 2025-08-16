using FluentValidation;
using LoyaltyApi.RequestModels;

namespace LoyaltyApi.Validators;

public class RegisterRequestBodyValidator : AbstractValidator<RegisterRequestBody>
{
    public RegisterRequestBodyValidator()
    {
        // Email validation
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
        
        // Restaurant ID validation
        RuleFor(x => x.RestaurantId)
            .GreaterThan(0).WithMessage("Restaurant Id must be greater than 0.");
        
        // Name validation
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters long.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
        
    }
}
