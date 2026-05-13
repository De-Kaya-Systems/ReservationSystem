namespace DeKayaServer.Contracts.CustomerAccount;

public sealed class CustomerAccountPaymentHistoryItemDto
{
    public Guid Id { get; set; }
    public string SourceType { get; set; } = default!;
    public Guid? SourceId { get; set; }

    public Guid PaymentTypeId { get; set; }
    public string? PaymentTypeName { get; set; }

    public decimal PaymentAmount { get; set; }
    public decimal RemainingBalance { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Notes { get; set; }
}
