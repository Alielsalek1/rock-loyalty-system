using System.ComponentModel.DataAnnotations;

namespace LoyaltyApi.Models
{
    public class Token
    {
        public string TokenValue { get; set; }

        public TokenType TokenType { get; set; }

        public int CustomerId { get; set; }

        public Role Role { get; set; }

        public int RestaurantId
        { get; set; }

        public Token() { }
    }
}