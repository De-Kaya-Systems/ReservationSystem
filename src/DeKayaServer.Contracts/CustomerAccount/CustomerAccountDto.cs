namespace DeKayaServer.Contracts.CustomerAccount;

public sealed class CustomerAccountDto
{
    public Guid CustomerId { get; set; }
    public string CustomerFullName { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }

    public DateTime? PeriodStartDate { get; set; }
    public DateTime? PeriodEndDate { get; set; }

    public decimal CurrentOutstandingAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public DateTime? LastPaymentDate { get; set; }

    public decimal OpeningBalance { get; set; }
    public decimal PeriodDebtAmount { get; set; }
    public decimal PeriodPaidAmount { get; set; }
    public decimal PeriodAdjustmentAmount { get; set; }
    public decimal PeriodNetAmount { get; set; }
    public decimal ClosingBalance { get; set; }

    public List<CustomerAccountStatementItemDto> StatementItems { get; set; } = [];
    public List<CustomerAccountBalanceItemDto> BalanceItems { get; set; } = [];
    public List<CustomerAccountPaymentHistoryItemDto> PaymentHistories { get; set; } = [];
}
