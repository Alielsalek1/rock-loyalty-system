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


        RuleFor(x => x.CreditPointsLifeTime)
            .GreaterThan(0).WithMessage("Credit Points Lifetime must be greater than 0.");


        RuleFor(x => x.VoucherLifeTime)
            .GreaterThan(0).WithMessage("Voucher Lifetime must be greater than 0.");

        RuleFor(x => x.VoucherMinValue)
            .GreaterThanOrEqualTo(10).WithMessage("Voucher Minimum Value must be greater than 10.");
    }
}
