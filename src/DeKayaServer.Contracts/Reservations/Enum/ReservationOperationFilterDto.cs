namespace DeKayaServer.Contracts.Reservations.Enum;

public enum ReservationOperationFilterDto
{
    Reserved = 1,
    WaitingDelivery = 2,
    DeliveredToCustomer = 3,
    WaitingPickup = 4,
    PickedUpFromCustomer = 5,
    Completed = 6
}
