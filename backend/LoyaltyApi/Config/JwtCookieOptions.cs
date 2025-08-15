namespace LoyaltyApi.Config
{
    public class JwtCookieOptions : CookieOptions
    {
        public new bool HttpOnly { get; set; }
        public new bool Secure { get; set; }
        public new SameSiteMode SameSite { get; set; }
    }
}