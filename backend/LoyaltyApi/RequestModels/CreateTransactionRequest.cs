using LoyaltyApi.Models;

namespace LoyaltyApi.RequestModels;

public class CreateTransactionRequest
{
    public int CustomerId { get; set; }
    public int RestaurantId { get; set; }
    public int ReceiptId { get; set; }
    public double Amount { get; set; }
    public DateTime? TransactionDate { get; set; }
}