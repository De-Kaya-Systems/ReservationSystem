namespace DeKayaServer.Contracts.Dashboard;

public sealed class DashboardFinanceSummaryDto
{
    public decimal TotalBilledAmount { get; set; }
    public decimal TotalCollectedAmount { get; set; }
    public decimal OutstandingDebtAmount { get; set; }
}