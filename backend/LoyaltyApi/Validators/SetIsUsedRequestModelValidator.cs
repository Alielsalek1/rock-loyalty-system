using FluentValidation;
using LoyaltyApi.RequestModels;

namespace LoyaltyApi.Validators;

public class SetIsUsedRequestModelValidator : AbstractValidator<SetIsUsedRequestModel>
{
    public SetIsUsedRequestModelValidator()
    {
        RuleFor(x => x.RestaurantId)
            .GreaterThan(0).WithMessage("Restaurant Id must be greater than 0.");
        
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Customer Id must be greater than 0.");
        
        // IsUsed is a boolean, so no additional validation needed beyond the type constraint
    }
}
