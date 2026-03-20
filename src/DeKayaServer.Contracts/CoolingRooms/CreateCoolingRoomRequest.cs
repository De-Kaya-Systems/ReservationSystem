namespace DeKayaServer.Contracts.CoolingRooms;

public sealed class CreateCoolingRoomRequest
{
    public string RoomName { get; init; } = default!;
    public decimal DailyPrice { get; init; }
    public bool Shelf { get; init; }
    public bool IsActive { get; init; }
}
