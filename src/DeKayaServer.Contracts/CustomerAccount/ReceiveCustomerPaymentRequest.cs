namespace DeKayaServer.Contracts.CustomerAccount;

public sealed class ReceiveCustomerPaymentRequest
{
    public Guid CustomerBalanceId { get; set; }
    public Guid PaymentTypeId { get; set; }
    public decimal PaymentAmount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Notes { get; set; }
}
