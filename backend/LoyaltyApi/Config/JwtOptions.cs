namespace LoyaltyApi.Config
{
    public class JwtOptions
    {
        public required string SigningKey { get; set; }
        public required int ExpirationInMinutes { get; set; }

        public required JwtCookieOptions JwtCookieOptions { get; set; }
    }
}