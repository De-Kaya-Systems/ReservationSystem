namespace DeKayaServer.Contracts.CoolingRooms;

public sealed class CoolingRoomOverviewDto
{
    public Guid Id { get; set; }

    public string RoomName { get; set; } = default!;
    public decimal DailyPrice { get; set; }
    public bool Shelf { get; set; }
    public bool IsActive { get; set; }

    public Guid StatusId { get; set; }
    public string? StatusName { get; set; }

    public Guid? CurrentReservationId { get; set; }
    public string? CurrentReservationNumber { get; set; }
    public string? CurrentCustomerName { get; set; }
    public string? CurrentCustomerPhoneNumber { get; set; }
    public DateTime? CurrentReservationStart { get; set; }
    public DateTime? CurrentReservationEnd { get; set; }
    public int? CurrentReservationStatus { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? PickedUpAt { get; set; }

    public Guid? NextReservationId { get; set; }
    public string? NextReservationNumber { get; set; }
    public string? NextCustomerName { get; set; }
    public DateTime? NextReservationStart { get; set; }
    public DateTime? NextReservationEnd { get; set; }

    public DateTime? NextAvailableAt { get; set; }

    public decimal OutstandingAmount { get; set; }

    public Guid? MaintenanceId { get; set; }
    public string? MaintenanceDescription { get; set; }
    public DateTime? MaintenanceEnd { get; set; }
}
