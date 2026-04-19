namespace DeKayaServer.Contracts.Reservations;

public class CreateReservationRequest
{
    public Guid CustomerId { get; set; }
    public string DeliveryLocation { get; set; } = null!;
    public DateOnly DeliveryDate { get; set; }
    public TimeOnly DeliveryTime { get; set; }
    public DateOnly PickUpDate { get; set; }
    public TimeOnly PickUpTime { get; set; }
    public Guid CoolingRoomId { get; set; }
    public Guid PaymentTypeId { get; set; }
    public decimal PaidAtReservation { get; set; }
    public string? Note { get; set; }
}