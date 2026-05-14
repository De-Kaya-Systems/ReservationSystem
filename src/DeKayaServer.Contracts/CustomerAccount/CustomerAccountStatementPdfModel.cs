using DeKayaServer.Contracts.Pdf;

namespace DeKayaServer.Contracts.CustomerAccount;

public sealed class CustomerAccountStatementPdfModel
{
    public PdfDocumentInfoDto DocumentInfo { get; set; } = default!;

    public Guid CustomerId { get; set; }
    public string CustomerFullName { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }

    public decimal OpeningBalance { get; set; }
    public decimal PeriodDebtAmount { get; set; }
    public decimal PeriodPaidAmount { get; set; }
    public decimal PeriodAdjustmentAmount { get; set; }
    public decimal ClosingBalance { get; set; }

    public List<CustomerAccountStatementPdfLineModel> Lines { get; set; } = [];
}
