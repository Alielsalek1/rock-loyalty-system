namespace LoyaltyApi.RequestModels
{
    public class RestaurantCreditPointsRequestModel
    {
        public double? CreditPointsBuyingRate { get; set; }
        public double? CreditPointsSellingRate { get; set; }
        public int? CreditPointsLifeTime { get; set; }
        public int? VoucherLifeTime { get; set; }
    }
}