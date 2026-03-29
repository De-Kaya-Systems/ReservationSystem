namespace DeKayaServer.Contracts.CoolingRooms;

public sealed class CoolingRoomMaintenanceLogDto
{
    public Guid MaintenanceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateOnly MaintenanceDateStart { get; set; }
    public DateOnly MaintenanceDateEnd { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public Guid CoolingRoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;

    public Guid StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public string CreatedFullName { get; set; } = string.Empty;

    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public string? UpdatedFullName { get; set; }
}