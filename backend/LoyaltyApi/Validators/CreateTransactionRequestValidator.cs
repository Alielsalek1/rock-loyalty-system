using FluentValidation;
using LoyaltyApi.RequestModels;

namespace LoyaltyApi.Validators;

public class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Customer Id must be greater than 0.");
        
        RuleFor(x => x.RestaurantId)
            .GreaterThan(0).WithMessage("Restaurant Id must be greater than 0.");
        
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.");
    }
}
