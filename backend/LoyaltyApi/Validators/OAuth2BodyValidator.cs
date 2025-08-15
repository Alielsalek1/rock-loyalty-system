using FluentValidation;
using LoyaltyApi.RequestModels;

namespace LoyaltyApi.Validators;

public class OAuth2BodyValidator : AbstractValidator<OAuth2Body>
{
    public OAuth2BodyValidator()
    {
        RuleFor(x => x.RestaurantId)
            .GreaterThan(0).WithMessage("Restaurant Id must be greater than 0.");
        
        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage("Access token is required.");
    }
}
