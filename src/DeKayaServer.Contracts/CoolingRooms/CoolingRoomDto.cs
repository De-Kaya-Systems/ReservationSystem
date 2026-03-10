using DeKayaServer.Contracts.Dto;

namespace DeKayaServer.Contracts.CoolingRooms;

public sealed class CoolingRoomDto : EntityDto
{
    public string RoomName { get; set; } = default!;
    public decimal DailyPrice { get; set; }
    public bool Shelf { get; set; }
    public Guid StatusId { get; set; }
    public string? StatusName { get; set; }
    public Guid? MaintenanceId { get; set; }
}