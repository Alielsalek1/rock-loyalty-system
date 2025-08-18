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

        // Fields to replace CreditPointsTransactionDetail functionality
        public int? EarnTransactionId { get; set; } // For spend/expire transactions: which earn transaction they're using
        public int RemainingPoints { get; set; } = 0; // For earn transactions: how many points are still available

        // Navigation properties
        // Commented out to remove transaction details dependency
        // public ICollection<CreditPointsTransactionDetail> CreditPointsTransactionDetails { get; set; }
    }
}