namespace LoyaltyApi.Models;

public class PagedVouchersResponse
{
    public List<Voucher> Vouchers { get; set; }
    
    public PaginationMetadata PaginationMetadata { get; set; }
}