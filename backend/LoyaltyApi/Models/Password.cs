namespace LoyaltyApi.Models
{
    public class Password
    {
        public required int CustomerId { get; set; }
        public required int RestaurantId { get; set; }
        public string? Value { get; set; }
        public bool ConfirmedEmail { get; set; }
    }
}