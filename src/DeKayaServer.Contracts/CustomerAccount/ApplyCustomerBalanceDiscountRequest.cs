namespace DeKayaServer.Contracts.CustomerAccount;

public sealed class ApplyCustomerBalanceDiscountRequest
{
    public decimal Amount { get; set; }
    public string Reason { get; set; } = default!;
    public string? Note { get; set; }
}
