using FluentValidation;
using LoyaltyApi.RequestModels;

namespace LoyaltyApi.Validators;

public class RestaurantCreditPointsRequestModelValidator : AbstractValidator<RestaurantCreditPointsRequestModel>
{
    public RestaurantCreditPointsRequestModelValidator()
    {
        // At least one property should be provided for update
        RuleFor(x => x)
            .Must(x => x.CreditPointsBuyingRate.HasValue || x.CreditPointsSellingRate.HasValue || 
                      x.CreditPointsLifeTime.HasValue || x.VoucherLifeTime.HasValue || x.VoucherMinValue.HasValue)
            .WithMessage("At least one property must be provided for update.");
        
        // If rates are provided, validate them
        When(x => x.CreditPointsBuyingRate.HasValue, () => {
            RuleFor(x => x.CreditPointsBuyingRate!.Value)
                .GreaterThan(0).WithMessage("Credit Points Buying Rate must be greater than 0.");
        });
        
        When(x => x.CreditPointsSellingRate.HasValue, () => {
            RuleFor(x => x.CreditPointsSellingRate!.Value)
                .GreaterThan(0).WithMessage("Credit Points Selling Rate must be greater than 0.");
        });
        
        When(x => x.CreditPointsLifeTime.HasValue, () => {
            RuleFor(x => x.CreditPointsLifeTime!.Value)
                .GreaterThan(0).WithMessage("Credit Points Lifetime must be greater than 0.");
        });
        
        When(x => x.VoucherLifeTime.HasValue, () => {
            RuleFor(x => x.VoucherLifeTime!.Value)
                .GreaterThan(0).WithMessage("Voucher Lifetime must be greater than 0.");
        });
    }
}
