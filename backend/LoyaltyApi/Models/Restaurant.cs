using System.ComponentModel.DataAnnotations;

namespace LoyaltyApi.Models;

public class Restaurant
{
    [Key]
    public required int RestaurantId { get; set; }

    // Rates for Credit Points and Loyalty Points
    public double CreditPointsBuyingRate { get; set; }
    public double CreditPointsSellingRate { get; set; }
    public double LoyaltyPointsBuyingRate { get; set; }
    public double LoyaltyPointsSellingRate { get; set; }

    // Lifetime values in Days for Credit Points and Loyalty Points
    public int CreditPointsLifeTime { get; set; }
    public int LoyaltyPointsLifeTime { get; set; }

    // Lifetime value in minutes for Vouchers
    public int VoucherLifeTime { get; set; }
}
