namespace DeKayaServer.Contracts.Dashboard;

public sealed class DashboardSummaryDto
{
    public DashboardFinanceSummaryDto Finance { get; set; } = new();
    public DashboardCoolingRoomSummaryDto CoolingRooms { get; set; } = new();
}