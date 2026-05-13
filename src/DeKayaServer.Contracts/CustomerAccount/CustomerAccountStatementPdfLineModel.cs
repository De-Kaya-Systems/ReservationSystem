namespace DeKayaServer.Contracts.CustomerAccount;

public sealed class CustomerAccountStatementPdfLineModel
{
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = default!;
    public string SourceType { get; set; } = default!;
    public string? ReservationNumber { get; set; }
    public string? Description { get; set; }

    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal BalanceAfterTransaction { get; set; }
}