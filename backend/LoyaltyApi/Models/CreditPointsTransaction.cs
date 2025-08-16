using System.Numerics;

namespace LoyaltyApi.Models
{
    public class CreditPointsTransaction
    {
        public int TransactionId { get; set; }

        public long ReceiptId { get; set; }

        public required int CustomerId { get; set; }

        public required int RestaurantId { get; set; }

        public required int Points { get; set; }

        public required double TransactionValue { get; set; }

        public bool IsExpired { get; set; } = false;

        public required TransactionType TransactionType { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.Now;


        // Navigation properties
        public Restaurant Restaurant { get; set; }

        public ICollection<CreditPointsTransactionDetail> CreditPointsTransactionDetails { get; set; }
    }
}