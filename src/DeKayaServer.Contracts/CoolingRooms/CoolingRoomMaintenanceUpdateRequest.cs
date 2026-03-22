namespace DeKayaServer.Contracts.CoolingRooms;

public sealed class CoolingRoomMaintenanceUpdateRequest
{
    public bool Remove { get; init; }
    public string? Description { get; init; }
    public DateTime MaintenanceDateStart { get; init; }
    public DateTime MaintenanceDateEnd { get; init; }
}