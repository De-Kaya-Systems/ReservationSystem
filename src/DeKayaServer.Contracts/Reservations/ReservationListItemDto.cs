using DeKayaServer.Contracts.Reservations.Enum;

namespace DeKayaServer.Contracts.Reservations;

public sealed class ReservationListItemDto
{
    public Guid Id { get; set; }
    public string? ReservationNumber { get; set; }

    public Guid CustomerId { get; set; }
    public string CustomerFullName { get; set; } = default!;

    public Guid CoolingRoomId { get; set; }
    public string CoolingRoomName { get; set; } = default!;

    public DateTime ReservationStart { get; set; }
    public DateTime ReservationEnd { get; set; }

    public int Status { get; set; }

    public ReservationOperationFilterDto? OperationStatus { get; set; }
    public string OperationStatusText { get; set; } = default!;

    public DateTime? DeliveredAt { get; set; }
    public DateTime? PickedUpAt { get; set; }

    public int TotalDays { get; set; }
    public decimal BaseDailyPrice { get; set; }
    public decimal AppliedDailyPrice { get; set; }
    public bool HasPriceOverride { get; set; }
    public string? PriceOverrideReason { get; set; }
    public decimal TotalAmount { get; set; }
}