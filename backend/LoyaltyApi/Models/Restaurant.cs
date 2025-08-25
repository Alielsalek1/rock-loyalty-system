using System.ComponentModel.DataAnnotations;

namespace LoyaltyApi.Models;

public class Restaurant
{
    [Key]
    public required int RestaurantId { get; set; }

    // Rates for Credit Points and Loyalty Points
    public double CreditPointsBuyingRate { get; set; }
    public double CreditPointsSellingRate { get; set; }



    // Lifetime values in Days for Credit Points and Loyalty Points
    public int CreditPointsLifeTime { get; set; }


    // Lifetime value in minutes for Vouchers
    public int VoucherLifeTime { get; set; }
}
