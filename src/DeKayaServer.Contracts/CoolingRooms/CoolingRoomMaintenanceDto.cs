namespace DeKayaServer.Contracts.CoolingRooms;

public sealed class CoolingRoomMaintenanceDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = default!;
    public DateOnly MaintenanceDateStart { get; set; }
    public DateOnly MaintenanceDateEnd { get; set; }
    public Guid StatusId { get; set; }
}