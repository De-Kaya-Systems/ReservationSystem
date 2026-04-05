using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.Reservations.ValueObjects;

namespace DeKayaServer.Domain.Reservations;

public sealed class Reservation : Entity
{
    private Reservation() { }
    private Reservation(
        IdentityId customerId,
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
        SetDeliveryLocation( deliveryLocation );

        SetDeliveryDate( deliveryDate );
        SetDeliveryTime( deliveryTime );
        SetPickUpDate( pickUpDate );
        SetPickUpTime( pickUpTime );

        SetCoolingRoomId( coolingRoomId );
        SetCoolingRoomDailyPrice( coolingRoomDailyPrice );

        SetPaidAtReservation( paidAtReservation );
        SetNote( note );

        SetTotalDay();
        SetReservationTotalAmount();
    }

    public static Reservation Create(
        IdentityId customerId,
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

    public IdentityId CustomerId { get; private set; } = default!;
    public DeliveryLocation DeliveryLocation { get; private set; } = default!;
    public DeliveryDate DeliveryDate { get; private set; } = default!;
    public DeliveryTime DeliveryTime { get; private set; } = default!;
    public PickUpDate PickUpDate { get; private set; } = default!;
    public PickUpTime PickUpTime { get; private set; } = default!;
    public TotalDay TotalDay { get; private set; } = default!;
    public IdentityId CoolingRoomId { get; private set; } = default!;
    public CoolingRoomDailyPrice CoolingRoomDailyPrice { get; private set; } = default!;
    public ReservationTotalAmount ReservationTotalAmount { get; private set; } = default!;
    public Note? Note { get; private set; }
    public PaidAtReservation PaidAtReservation { get; private set; } = new( 0 );

    #region Behavior
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
