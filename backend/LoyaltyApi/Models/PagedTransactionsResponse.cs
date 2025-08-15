namespace LoyaltyApi.Models;


public class PagedTransactionsResponse
{
    public List<CreditPointsTransaction> Transactions { get; set; }
    public PaginationMetadata PaginationMetadata { get; set; }
}