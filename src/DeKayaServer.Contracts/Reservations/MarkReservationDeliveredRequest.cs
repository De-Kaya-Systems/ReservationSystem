namespace DeKayaServer.Contracts.Reservations;

public sealed class MarkReservationDeliveredRequest
{
    public Guid Id { get; set; }
    public DateTime DeliveredAt { get; set; }
}
