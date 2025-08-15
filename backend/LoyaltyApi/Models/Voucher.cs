using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace LoyaltyApi.Models
{
    public class Voucher
    {
        [ForeignKey("Restaurant")]
        public required int RestaurantId { get; set; }
        public required int CustomerId { get; set; }

        public string? LongCode { get; set; }

        public string? ShortCode { get; set; }
        public int? Value { get; set; }
        public DateTime DateOfCreation { get; set; }
        public bool IsUsed { get; set; }
    }
}