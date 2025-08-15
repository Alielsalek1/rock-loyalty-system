using FluentValidation;
using LoyaltyApi.RequestModels;

namespace LoyaltyApi.Validators;

public class CreateRestaurantRequestModelValidator : AbstractValidator<CreateRestaurantRequestModel>
{
    public CreateRestaurantRequestModelValidator()
    {
        RuleFor(x => x.RestaurantId)
            .GreaterThan(0).WithMessage("Restaurant Id must be greater than 0.");
        
        RuleFor(x => x.CreditPointsBuyingRate)
            .GreaterThan(0).WithMessage("Credit Points Buying Rate must be greater than 0.");
        
        RuleFor(x => x.CreditPointsSellingRate)
            .GreaterThan(0).WithMessage("Credit Points Selling Rate must be greater than 0.");
        
        RuleFor(x => x.LoyaltyPointsBuyingRate)
            .GreaterThan(0).WithMessage("Loyalty Points Buying Rate must be greater than 0.");
        
        RuleFor(x => x.LoyaltyPointsSellingRate)
            .GreaterThan(0).WithMessage("Loyalty Points Selling Rate must be greater than 0.");
        
        RuleFor(x => x.CreditPointsLifeTime)
            .GreaterThan(0).WithMessage("Credit Points Lifetime must be greater than 0.");
        
        RuleFor(x => x.LoyaltyPointsLifeTime)
            .GreaterThan(0).WithMessage("Loyalty Points Lifetime must be greater than 0.");
        
        RuleFor(x => x.VoucherLifeTime)
            .GreaterThan(0).WithMessage("Voucher Lifetime must be greater than 0.");
    }
}
