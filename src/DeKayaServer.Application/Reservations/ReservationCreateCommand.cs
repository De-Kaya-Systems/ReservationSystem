using DeKayaServer.Application.Behaviors;
using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.CoolingRooms;
using DeKayaServer.Domain.CoolingRoomStatus;
using DeKayaServer.Domain.CustomerBalance;
using DeKayaServer.Domain.CustomerBalance.ValueObjects;
using DeKayaServer.Domain.Customers;
using DeKayaServer.Domain.Reservations;
using DeKayaServer.Domain.Reservations.ValueObjects;
using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Reservations;

[Permission( "reservation:create" )]
public sealed record ReservationCreateCommand(
    Guid CustomerId,
    string DeliveryLocation,
    DateOnly DeliveryDate,
    TimeOnly DeliveryTime,
    DateOnly PickUpDate,
    TimeOnly PickUpTime,
    Guid CoolingRoomId,
    decimal PaidAtReservation,
    string? Note ) : IRequest<Result<string>>;

public sealed class ReservationCreateCommandValidator : AbstractValidator<ReservationCreateCommand>
{
    public ReservationCreateCommandValidator()
    {
        RuleFor( x => x.CustomerId )
            .NotEmpty()
            .WithMessage( "Geçerli bir müşteri seçin!" );

        RuleFor( x => x.CoolingRoomId )
            .NotEmpty()
            .WithMessage( "Geçerli bir soğuk oda seçin!" );

        RuleFor( x => x.DeliveryLocation )
            .NotEmpty()
            .WithMessage( "Teslimat konumu boş olamaz." );

        RuleFor( x => x.PaidAtReservation )
            .GreaterThanOrEqualTo( 0 )
            .WithMessage( "Alınan ödeme negatif olamaz." );

        RuleFor( x => x )
            .Must( x =>
            {
                var delivery = x.DeliveryDate.ToDateTime( x.DeliveryTime );
                var pickup = x.PickUpDate.ToDateTime( x.PickUpTime );
                return pickup >= delivery;
            } )
            .WithMessage( "Geri alım tarihi/saati teslim tarih/saatinden önce olamaz." );
    }
}

internal sealed class ReservationCreateCommandHandler(
    IReservationRepository reservationRepository,
    ICustomerRepository customerRepository,
    ICoolingRoomRepository coolingRoomRepository,
    ICoolingRoomStatusRepository coolingRoomStatusRepository,
    ICustomerBalanceRepository customerBalanceRepository,
    IUnitOfWork unitOfWork ) : IRequestHandler<ReservationCreateCommand, Result<string>>
{
    public const string AvailableCoolingRoomStatus = "Uygun";
    public const string BookedCoolingRoomStatus = "Booked";
    public const string ReservedCoolingRoomStatus = "Rezerve";
    public async Task<Result<string>> Handle( ReservationCreateCommand request, CancellationToken cancellationToken )
    {
        var customer = await customerRepository.FirstOrDefaultAsync( x => x.Id == request.CustomerId, cancellationToken );
        if ( customer is null )
        {
            return Result<string>.Failure( "Müşteri bulunamadı!" );
        }

        var coolingRoom = await coolingRoomRepository.FirstOrDefaultAsync( x => x.Id == request.CoolingRoomId, cancellationToken );
        if ( coolingRoom is null )
        {
            return Result<string>.Failure( "Soğuk oda bulunamadı!" );
        }

        var currentStatus = await coolingRoomStatusRepository.FirstOrDefaultAsync( x => x.Id == coolingRoom.RoomStatusId, cancellationToken );
        if ( currentStatus is null )
        {
            return Result<string>.Failure( "Soğuk oda durumu bulunamadı!" );
        }

        if ( currentStatus.StatusName.Value != AvailableCoolingRoomStatus )
        {
            return Result<string>.Failure( "Seçilen soğuk oda uygun değil!" );
        }

        var hasOverlap = await reservationRepository.AnyAsync(
            x => x.CoolingRoomId == request.CoolingRoomId
            && x.DeliveryDate.Value <= request.PickUpDate
            && request.DeliveryDate <= x.PickUpDate.Value, cancellationToken );

        if ( hasOverlap )
        {
            return Result<string>.Failure( "Seçilen soğuk oda bu tarihler arasında rezerve edilmiş!" );
        }

        var reservation = Reservation.Create(
            customerId: new IdentityId( request.CustomerId ),
            deliveryLocation: new DeliveryLocation( request.DeliveryLocation ),
            deliveryDate: new DeliveryDate( request.DeliveryDate ),
            deliveryTime: new DeliveryTime( request.DeliveryTime ),
            pickUpDate: new PickUpDate( request.PickUpDate ),
            pickUpTime: new PickUpTime( request.PickUpTime ),
            coolingRoomId: new IdentityId( request.CoolingRoomId ),
            coolingRoomDailyPrice: new CoolingRoomDailyPrice( coolingRoom.DailyPrice.Value ),
            paidAtReservation: new PaidAtReservation( request.PaidAtReservation ),
            note: string.IsNullOrWhiteSpace( request.Note ) ? null : new Note( request.Note ) );

        var totalAmount = reservation.ReservationTotalAmount.Value;
        var paidAmount = reservation.PaidAtReservation.Value;

        if ( paidAmount > totalAmount )
        {
            return Result<string>.Failure( "Alınan ödeme toplam rezervasyon tutarından büyük olamaz." );
        }

        reservationRepository.Add( reservation );

        var deliveryDateTime = request.DeliveryDate.ToDateTime( request.DeliveryTime );
        var targetStatusName = deliveryDateTime > DateTime.Now ? ReservedCoolingRoomStatus : BookedCoolingRoomStatus;

        var targetStatus = await coolingRoomStatusRepository
           .FirstOrDefaultAsync( x => x.StatusName.Value == targetStatusName, cancellationToken );

        if ( targetStatus is null )
        {
            return Result<string>.Failure( $"Durum bulunamadı: {targetStatusName}" );
        }

        coolingRoom.SetRoomStatusId( targetStatus.Id );
        coolingRoomRepository.Update( coolingRoom );


        var outstandingBalance = totalAmount - paidAmount;
        if ( outstandingBalance > 0 )
        {
            var customerBalance = new CustomerBalance(
                customerId: new IdentityId( request.CustomerId ),
                totalAmount: new TotalAmount( totalAmount ),
                outstandingAmount: new OutstandingAmount( outstandingBalance ),
                paidAmount: new PaidAmount( paidAmount ),
                description: new Description( $"Rezervasyon borcu - RezervasyonId: {reservation.Id.Value}" ),
                paymentType: null, //buna bakacagim daha sonra
                lastPaymentAt: paidAmount > 0 ? new LastPaymentAt( DateTime.Now ) : null,
                reservationId: reservation.Id );

            customerBalanceRepository.Add( customerBalance );
        }
        await unitOfWork.SaveChangesAsync( cancellationToken );
        return "Rezervasyon başarıyla oluşturuldu";
    }
}