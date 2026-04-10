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

[Permission( "reservation:edit" )]
public sealed record ReservationUpdateCommand(
    Guid Id,
    Guid CustomerId,
    string DeliveryLocation,
    DateOnly DeliveryDate,
    TimeOnly DeliveryTime,
    DateOnly PickUpDate,
    TimeOnly PickUpTime,
    Guid CoolingRoomId,
    decimal PaidAtReservation,
    string? Note ) : IRequest<Result<string>>;

public sealed class ReservationUpdateCommandValidator : AbstractValidator<ReservationUpdateCommand>
{
    public ReservationUpdateCommandValidator()
    {
        RuleFor( x => x.Id )
            .NotEmpty()
            .WithMessage( "Geçerli bir rezervasyon seçin!" );

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

internal sealed class ReservationUpdateCommandHandler(
    IReservationRepository reservationRepository,
    ICustomerRepository customerRepository,
    ICoolingRoomRepository coolingRoomRepository,
    ICoolingRoomStatusRepository coolingRoomStatusRepository,
    ICustomerBalanceRepository customerBalanceRepository,
    IUnitOfWork unitOfWork ) : IRequestHandler<ReservationUpdateCommand, Result<string>>
{
    private const string AvailableCoolingRoomStatus = "Uygun";
    private const string BookedCoolingRoomStatus = "Booked";
    private const string ReservedCoolingRoomStatus = "Rezerve";

    public async Task<Result<string>> Handle( ReservationUpdateCommand request, CancellationToken cancellationToken )
    {
        var reservation = await reservationRepository.FirstOrDefaultAsync( x => x.Id == request.Id, cancellationToken );
        if ( reservation is null )
        {
            return Result<string>.Failure( "Rezervasyon bulunamadı!" );
        }

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

        var changingRoom = reservation.CoolingRoomId.Value != request.CoolingRoomId;

        if ( changingRoom )
        {
            var currentStatus = await coolingRoomStatusRepository
                .FirstOrDefaultAsync( x => x.Id == coolingRoom.RoomStatusId, cancellationToken );

            if ( currentStatus is null )
            {
                return Result<string>.Failure( "Soğuk oda durumu bulunamadı!" );
            }

            if ( currentStatus.StatusName.Value != AvailableCoolingRoomStatus )
            {
                return Result<string>.Failure( "Seçilen soğuk oda uygun değil!" );
            }
        }

        var hasOverlap = await reservationRepository.AnyAsync(
            x => x.Id != request.Id
                && x.CoolingRoomId == request.CoolingRoomId
                && x.DeliveryDate.Value <= request.PickUpDate
                && request.DeliveryDate <= x.PickUpDate.Value,
            cancellationToken );

        if ( hasOverlap )
        {
            return Result<string>.Failure( "Seçilen soğuk oda bu tarihler arasında rezerve edilmiş!" );
        }

        reservation.SetCustomerId( new IdentityId( request.CustomerId ) );
        reservation.SetDeliveryLocation( new DeliveryLocation( request.DeliveryLocation ) );
        reservation.SetDeliveryDate( new DeliveryDate( request.DeliveryDate ) );
        reservation.SetDeliveryTime( new DeliveryTime( request.DeliveryTime ) );
        reservation.SetPickUpDate( new PickUpDate( request.PickUpDate ) );
        reservation.SetPickUpTime( new PickUpTime( request.PickUpTime ) );
        reservation.SetCoolingRoomId( new IdentityId( request.CoolingRoomId ) );
        reservation.SetCoolingRoomDailyPrice( new CoolingRoomDailyPrice( coolingRoom.DailyPrice.Value ) );
        reservation.SetPaidAtReservation( new PaidAtReservation( request.PaidAtReservation ) );
        reservation.SetNote( string.IsNullOrWhiteSpace( request.Note ) ? null : new Note( request.Note ) );
        reservation.SetTotalDay();
        reservation.SetReservationTotalAmount();

        var totalAmount = reservation.ReservationTotalAmount.Value;
        var paidAmount = reservation.PaidAtReservation.Value;

        if ( paidAmount > totalAmount )
        {
            return Result<string>.Failure( "Alınan ödeme toplam rezervasyon tutarından büyük olamaz." );
        }

        reservationRepository.Update( reservation );

        var deliveryDateTime = request.DeliveryDate.ToDateTime( request.DeliveryTime );
        var targetStatusName = deliveryDateTime > DateTime.Now
            ? ReservedCoolingRoomStatus
            : BookedCoolingRoomStatus;

        var targetStatus = await coolingRoomStatusRepository
            .FirstOrDefaultAsync( x => x.StatusName.Value == targetStatusName, cancellationToken );

        if ( targetStatus is null )
        {
            return Result<string>.Failure( $"Durum bulunamadı: {targetStatusName}" );
        }

        coolingRoom.SetRoomStatusId( targetStatus.Id );
        coolingRoomRepository.Update( coolingRoom );

        var outstandingBalance = totalAmount - paidAmount;

        var customerBalance = await customerBalanceRepository.FirstOrDefaultAsync(
            x => x.ReservationId == reservation.Id,
            cancellationToken );

        if ( customerBalance is null && outstandingBalance > 0 )
        {
            customerBalance = new CustomerBalance(
                customerId: new IdentityId( request.CustomerId ),
                totalAmount: new TotalAmount( totalAmount ),
                outstandingAmount: new OutstandingAmount( outstandingBalance ),
                paidAmount: new PaidAmount( paidAmount ),
                description: new Description( $"Rezervasyon borcu - RezervasyonId: {reservation.Id.Value}" ),
                paymentType: null,
                lastPaymentAt: paidAmount > 0 ? new LastPaymentAt( DateTime.Now ) : null,
                reservationId: reservation.Id );

            customerBalanceRepository.Add( customerBalance );
        }
        else if ( customerBalance is not null )
        {
            customerBalance.SetCustomerId( new IdentityId( request.CustomerId ) );
            customerBalance.SetTotalAmount( new TotalAmount( totalAmount ) );
            customerBalance.SetPaidAmount( new PaidAmount( paidAmount ) );
            customerBalance.SetOutstandingAmount( new OutstandingAmount( outstandingBalance ) );
            customerBalance.SetDescription( new Description( $"Rezervasyon borcu - RezervasyonId: {reservation.Id.Value}" ) );
            customerBalance.SetLastPaymentAt( paidAmount > 0 ? new LastPaymentAt( DateTime.Now ) : null );
            customerBalanceRepository.Update( customerBalance );
        }

        await unitOfWork.SaveChangesAsync( cancellationToken );
        return "Rezervasyon başarıyla güncellendi";
    }
}