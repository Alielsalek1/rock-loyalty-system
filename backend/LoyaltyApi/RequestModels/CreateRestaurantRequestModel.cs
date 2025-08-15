namespace LoyaltyApi.RequestModels
{
    public class CreateRestaurantRequestModel
    {
        public int RestaurantId { get; set; }
        public double CreditPointsBuyingRate { get; set; }
        public double CreditPointsSellingRate { get; set; }
        public double LoyaltyPointsBuyingRate { get; set; }
        public double LoyaltyPointsSellingRate { get; set; }

        public int CreditPointsLifeTime { get; set; }
        public int LoyaltyPointsLifeTime { get; set; }

        public int VoucherLifeTime { get; set; }
    }
}