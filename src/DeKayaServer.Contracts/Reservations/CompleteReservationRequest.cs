namespace DeKayaServer.Contracts.Reservations;

public sealed class CompleteReservationRequest
{
    public Guid Id { get; set; }

    public DateTime DeliveredAt { get; set; }
    public DateTime PickedUpAt { get; set; }

    public bool PaymentReceived { get; set; }
    public Guid? PaymentTypeId { get; set; }
    public decimal PaymentAmount { get; set; }

    public bool HasFault { get; set; }
    public string? FaultDescription { get; set; }

    public string? Note { get; set; }
}