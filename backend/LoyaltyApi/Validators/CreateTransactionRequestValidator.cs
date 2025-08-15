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
        
        RuleFor(x => x.ReceiptId)
            .GreaterThan(0).WithMessage("Receipt Id must be greater than 0.");
        
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.");
        
        // If TransactionDate is provided, validate it's not in the future
        When(x => x.TransactionDate.HasValue, () => {
            RuleFor(x => x.TransactionDate!.Value)
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Transaction date cannot be in the future.");
        });
    }
}
