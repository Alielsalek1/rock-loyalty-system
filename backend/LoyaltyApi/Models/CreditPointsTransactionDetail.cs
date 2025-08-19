namespace LoyaltyApi.Models;

public class CreditPointsTransactionDetail
{
    public int DetailId { get; set; }

    public required int TransactionId { get; set; }

    public required int EarnTransactionId { get; set; }

    public required int PointsUsed { get; set; }

    // Navigation properties
    public CreditPointsTransaction Transaction { get; set; }

    public CreditPointsTransaction EarnTransaction { get; set; }
}