using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.Reservations.Enum;
using DeKayaServer.Domain.Reservations.ValueObjects;

namespace DeKayaServer.Domain.Reservations;

public sealed class Reservation : Entity
{
    private Reservation() { }
    private Reservation(
        IdentityId customerId,
        ReservationNumber reservationNumber,
        DeliveryLocation deliveryLocation,
        DeliveryDate deliveryDate,
        DeliveryTime deliveryTime,
        PickUpDate pickUpDate,
        PickUpTime pickUpTime,
        IdentityId coolingRoomId,
        CoolingRoomDailyPrice coolingRoomDailyPrice,
        PaidAtReservation paidAtReservation,
        Note? note )
    {
        SetCustomerId( customerId );
        SetReservationNumber( reservationNumber );
        SetDeliveryLocation( deliveryLocation );

        SetDeliveryDate( deliveryDate );
        SetDeliveryTime( deliveryTime );
        SetPickUpDate( pickUpDate );
        SetPickUpTime( pickUpTime );

        SetCoolingRoomId( coolingRoomId );
        SetCoolingRoomDailyPrice( coolingRoomDailyPrice );

        SetPaidAtReservation( paidAtReservation );
        SetNote( note );

        SetStatus( ReservationStatus.Scheduled );

        SetTotalDay();
        SetReservationTotalAmount();
    }

    public static Reservation Create(
        IdentityId customerId,
        ReservationNumber reservationNumber,
        DeliveryLocation deliveryLocation,
        DeliveryDate deliveryDate,
        DeliveryTime deliveryTime,
        PickUpDate pickUpDate,
        PickUpTime pickUpTime,
        IdentityId coolingRoomId,
        CoolingRoomDailyPrice coolingRoomDailyPrice,
        PaidAtReservation paidAtReservation,
        Note? note )
    {
        var reservation = new Reservation(
            customerId,
            reservationNumber,
            deliveryLocation,
            deliveryDate,
            deliveryTime,
            pickUpDate,
            pickUpTime,
            coolingRoomId,
            coolingRoomDailyPrice,
            paidAtReservation,
            note );
        return reservation;
    }

    public ReservationNumber ReservationNumber { get; private set; } = default!;
    public IdentityId CustomerId { get; private set; } = default!;
    public DeliveryLocation DeliveryLocation { get; private set; } = default!;
    public DeliveryDate DeliveryDate { get; private set; } = default!;
    public DeliveryTime DeliveryTime { get; private set; } = default!;
    public PickUpDate PickUpDate { get; private set; } = default!;
    public PickUpTime PickUpTime { get; private set; } = default!;
    public ReservationStatus Status { get; private set; } = ReservationStatus.Scheduled;
    public DeliveredAt? DeliveredAt { get; private set; }
    public PickedUpAt? PickedUpAt { get; private set; }
    public TotalDay TotalDay { get; private set; } = default!;
    public IdentityId CoolingRoomId { get; private set; } = default!;
    public CoolingRoomDailyPrice CoolingRoomDailyPrice { get; private set; } = default!;
    public ReservationTotalAmount ReservationTotalAmount { get; private set; } = default!;
    public Note? Note { get; private set; }
    public PaidAtReservation PaidAtReservation { get; private set; } = new( 0 );

    #region Behavior
    public void SetReservationNumber( ReservationNumber reservationNumber )
    {
        ReservationNumber = reservationNumber;
    }
    public void SetCustomerId( IdentityId customerId )
    {
        CustomerId = customerId;
    }

    public void SetDeliveryLocation( DeliveryLocation deliveryLocation )
    {
        DeliveryLocation = deliveryLocation;
    }

    public void SetDeliveryDate( DeliveryDate deliveryDate )
    {
        DeliveryDate = deliveryDate;
    }

    public void SetDeliveryTime( DeliveryTime deliveryTime )
    {
        DeliveryTime = deliveryTime;
    }

    public void SetPickUpDate( PickUpDate pickUpDate )
    {
        PickUpDate = pickUpDate;
    }

    public void SetPickUpTime( PickUpTime pickUpTime )
    {
        PickUpTime = pickUpTime;
    }

    public void SetStatus( ReservationStatus status )
    {
        Status = status;
    }

    public void MarkAsDelivered()
    {
        Status = ReservationStatus.DeliveredToCustomer;
        DeliveredAt = new DeliveredAt( DateTime.Now );
    }

    public void MarkAsPickedUp()
    {
        Status = ReservationStatus.PickedUpFromCustomer;
        PickedUpAt = new PickedUpAt( DateTime.Now );
    }

    public void Complete()
    {
        Status = ReservationStatus.Completed;
    }

    public void Cancel()
    {
        Status = ReservationStatus.Cancelled;
    }

    public void CompleteReservation( DeliveredAt deliveredAt, PickedUpAt pickedUpAt )
    {
        DeliveredAt = deliveredAt;
        PickedUpAt = pickedUpAt;
        Status = ReservationStatus.Completed;
    }

    public void SetTotalDay()
    {
        var deliveryDateTime = DeliveryDate.Value.ToDateTime( DeliveryTime.Value );
        var pickUpDateTime = PickUpDate.Value.ToDateTime( PickUpTime.Value );

        var totalDays = ( pickUpDateTime.Date - deliveryDateTime.Date ).Days;
        var sameDayExtraAllowed = pickUpDateTime.TimeOfDay <= deliveryDateTime.TimeOfDay.Add( TimeSpan.FromHours( 2 ) );

        if ( totalDays == 0 || ( totalDays == 1 && sameDayExtraAllowed ) )
        {
            TotalDay = new TotalDay( 1 );
        }
        else if ( sameDayExtraAllowed )
        {
            TotalDay = new TotalDay( totalDays );
        }
        else
        {
            TotalDay = new TotalDay( totalDays + 1 );
        }
    }

    public void SetCoolingRoomId( IdentityId coolingRoomId )
    {
        CoolingRoomId = coolingRoomId;
    }

    public void SetCoolingRoomDailyPrice( CoolingRoomDailyPrice coolingRoomDailyPrice )
    {
        CoolingRoomDailyPrice = coolingRoomDailyPrice;
    }

    public void SetNote( Note? note )
    {
        Note = note;
    }

    public void SetPaidAtReservation( PaidAtReservation paidAtReservation )
    {
        PaidAtReservation = paidAtReservation;
    }

    public void SetReservationTotalAmount()
    {
        ReservationTotalAmount = new ReservationTotalAmount( CoolingRoomDailyPrice.Value * TotalDay.Value );
    }
    #endregion
}
