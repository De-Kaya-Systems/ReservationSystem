namespace DeKayaServer.Contracts.Reservations;

public enum ReservationOperationStatusDto
{
    Scheduled = 1,
    DeliveredToCustomer = 2,
    PickedUpFromCustomer = 3,
    Completed = 4,
    Cancelled = 5
}
