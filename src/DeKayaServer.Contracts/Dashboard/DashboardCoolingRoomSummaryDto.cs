namespace DeKayaServer.Contracts.Dashboard;

public sealed class DashboardCoolingRoomSummaryDto
{
    public int RoomsAtCustomerCount { get; set; }
    public int TodayDeliveriesCount { get; set; }
    public int TodayReturnsCount { get; set; }
}