using FluentValidation;
using LoyaltyApi.RequestModels;

namespace LoyaltyApi.Validators;

public class UpdateUserRequestModelValidator : AbstractValidator<UpdateUserRequestModel>
{
    public UpdateUserRequestModelValidator()
    {
        // At least one field must be provided for update
        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.Email) || !string.IsNullOrEmpty(x.PhoneNumber) || !string.IsNullOrEmpty(x.Name))
            .WithMessage("At least one field (Email, PhoneNumber, or Name) must be provided for update.");
        
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
        
        // If Name is provided, validate it
        When(x => !string.IsNullOrEmpty(x.Name), () => {
            RuleFor(x => x.Name!)
                .MinimumLength(2).WithMessage("Name must be at least 2 characters long.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
        });
    }
}
