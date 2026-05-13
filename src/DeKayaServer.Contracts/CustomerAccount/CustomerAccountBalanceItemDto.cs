namespace DeKayaServer.Contracts.CustomerAccount;

public sealed class CustomerAccountBalanceItemDto
{
    public Guid Id { get; set; }
    public string SourceType { get; set; } = default!;
    public Guid? SourceId { get; set; }

    public Guid PaymentTypeId { get; set; }
    public string? PaymentTypeName { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }

    public string BalanceStatus { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime? LastPaymentAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}