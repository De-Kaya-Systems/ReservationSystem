using DeKayaServer.Contracts.Dto;

namespace DeKayaServer.Contracts.Reservations;

public sealed class ReservationDto : EntityDto
{
    public string CustomerId { get; set; } = default!;
    public string DeliveryLocation { get; set; } = default!;
    public DateOnly DeliveryDate { get; set; } = default!;
    public DateTime DeliveryTime { get; set; } = default!;
    public DateOnly PickUpDate { get; set; } = default!;
    public DateTime PickUpTime { get; set; } = default!;
    public string CoolingRoomId { get; set; } = default!;
    public int TotalDay { get; set; }
    public decimal CoolingRoomDailyPrice { get; set; } = default!;
    public decimal? ReservationTotalAmount { get; set; }
    public string? Note { get; set; }
    public decimal? PaidAtReservation { get; set; }
}
