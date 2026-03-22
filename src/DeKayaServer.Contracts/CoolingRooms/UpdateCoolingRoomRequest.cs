namespace DeKayaServer.Contracts.CoolingRooms;

public sealed class UpdateCoolingRoomRequest
{
    public Guid Id { get; init; }
    public string RoomName { get; init; } = default!;
    public decimal DailyPrice { get; init; }
    public bool Shelf { get; init; }
    public bool IsActive { get; init; }
    public CoolingRoomMaintenanceUpdateRequest? Maintenance { get; init; }
}
