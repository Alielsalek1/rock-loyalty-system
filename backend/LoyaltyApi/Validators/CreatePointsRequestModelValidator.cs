using FluentValidation;
using LoyaltyApi.RequestModels;

namespace LoyaltyApi.Validators;

public class CreatePointsRequestModelValidator : AbstractValidator<CreatePointsRequestModel>
{
    public CreatePointsRequestModelValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Customer Id must be greater than 0.");
        
        RuleFor(x => x.RestaurantId)
            .GreaterThan(0).WithMessage("Restaurant Id must be greater than 0.");
        
        RuleFor(x => x.TransactionId)
            .GreaterThan(0).WithMessage("Transaction Id must be greater than 0.");
        
        RuleFor(x => x.CreditPoints)
            .GreaterThan(0).WithMessage("Credit Points must be greater than 0.");
    }
}
