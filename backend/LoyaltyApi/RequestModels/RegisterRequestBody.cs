namespace LoyaltyApi.RequestModels
{
    public class RegisterRequestBody
    {
        public string? PhoneNumber { get; set; }
        public required string Email { get; set; }
        public string? Password { get; set; }
        public required int RestaurantId { get; set; }
        public required string Name { get; set; }
    }
}