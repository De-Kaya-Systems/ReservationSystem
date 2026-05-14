using DeKayaServer.Application.Behaviors;
using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.Constants;
using DeKayaServer.Domain.CoolingRooms;
using DeKayaServer.Domain.CoolingRoomStatus;
using DeKayaServer.Domain.CustomerBalance;
using DeKayaServer.Domain.CustomerBalance.Enum;
using DeKayaServer.Domain.CustomerBalance.ValueObjects;
using DeKayaServer.Domain.Customers;
using DeKayaServer.Domain.PaymentHistory;
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
    decimal AppliedDailyPrice,
    string? PriceOverrideReason,
    string? PriceOverrideNote,
    Guid PaymentTypeId,
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

        RuleFor( x => x.AppliedDailyPrice )
            .GreaterThan( 0 )
            .WithMessage( "Uygulanan günlük fiyat sıfırdan büyük olmalıdır." );

        RuleFor( x => x.PriceOverrideReason )
            .MaximumLength( 250 )
            .WithMessage( "Fiyat değişikliği sebebi en fazla 250 karakter olabilir." );

        RuleFor( x => x.PriceOverrideNote )
            .MaximumLength( 500 )
            .WithMessage( "Fiyat değişikliği notu en fazla 500 karakter olabilir." );

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

        RuleFor( x => x.PaymentTypeId )
            .NotEmpty()
            .WithMessage( "Geçerli bir ödeme tipi seçin!" );
    }
}

internal sealed class ReservationCreateCommandHandler(
    IReservationRepository reservationRepository,
    ICustomerRepository customerRepository,
    ICoolingRoomRepository coolingRoomRepository,
    ICoolingRoomStatusRepository coolingRoomStatusRepository,
    ICustomerBalanceRepository customerBalanceRepository,
    IPaymentHistoryRepository paymentHistoryRepository,
    IUnitOfWork unitOfWork ) : IRequestHandler<ReservationCreateCommand, Result<string>>
{
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

        if ( IsFaultyStatus( currentStatus.StatusName.Value ) )
        {
            return Result<string>.Failure( "Seçilen soğuk oda arıza/bakımda!" );
        }

        var hasOverlap = await reservationRepository.AnyAsync(
            x => x.CoolingRoomId == request.CoolingRoomId
            && x.DeliveryDate.Value <= request.PickUpDate
            && request.DeliveryDate <= x.PickUpDate.Value,
            cancellationToken );

        if ( hasOverlap )
        {
            return Result<string>.Failure( "Seçilen soğuk oda bu tarihler arasında rezerve edilmiş!" );
        }

        var baseDailyPrice = coolingRoom.DailyPrice.Value;
        var appliedDailyPrice = request.AppliedDailyPrice;
        var hasPriceOverride = appliedDailyPrice != baseDailyPrice;

        if ( hasPriceOverride && string.IsNullOrWhiteSpace( request.PriceOverrideReason ) )
        {
            return Result<string>.Failure( "Fiyat değişikliği sebebi zorunludur." );
        }

        PriceOverrideReason? priceOverrideReason = hasPriceOverride
            ? new PriceOverrideReason( request.PriceOverrideReason!.Trim() )
            : null;

        PriceOverrideNote? priceOverrideNote = hasPriceOverride && !string.IsNullOrWhiteSpace( request.PriceOverrideNote )
            ? new PriceOverrideNote( request.PriceOverrideNote.Trim() )
            : null;

        var reservationCount = await reservationRepository.CountAsync( cancellationToken );
        var reservationNumber = new ReservationNumber( $"DK-{( reservationCount + 1 ):D6}" );

        var reservation = Reservation.Create(
            customerId: new IdentityId( request.CustomerId ),
            reservationNumber: reservationNumber,
            deliveryLocation: new DeliveryLocation( request.DeliveryLocation ),
            deliveryDate: new DeliveryDate( request.DeliveryDate ),
            deliveryTime: new DeliveryTime( request.DeliveryTime ),
            pickUpDate: new PickUpDate( request.PickUpDate ),
            pickUpTime: new PickUpTime( request.PickUpTime ),
            coolingRoomId: new IdentityId( request.CoolingRoomId ),
            coolingRoomBaseDailyPrice: new CoolingRoomBaseDailyPrice( baseDailyPrice ),
            coolingRoomDailyPrice: new CoolingRoomDailyPrice( appliedDailyPrice ),
            priceOverrideReason: priceOverrideReason,
            priceOverrideNote: priceOverrideNote,
            paidAtReservation: new PaidAtReservation( request.PaidAtReservation ),
            note: string.IsNullOrWhiteSpace( request.Note ) ? null : new Note( request.Note ) );

        var totalAmount = reservation.ReservationTotalAmount.Value;
        var paidAmount = reservation.PaidAtReservation.Value;

        if ( paidAmount > totalAmount )
        {
            return Result<string>.Failure( "Alınan ödeme toplam rezervasyon tutarından büyük olamaz." );
        }

        reservationRepository.Add( reservation );

        var targetStatusName = CoolingRoomStatusConstants.Reserved;

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
                sourceType: BalanceSourceType.Reservation,
                sourceId: reservation.Id,
                paymentTypeId: new IdentityId( request.PaymentTypeId ),
                totalAmount: new TotalAmount( totalAmount ),
                outstandingAmount: new OutstandingAmount( outstandingBalance ),
                paidAmount: new PaidAmount( paidAmount ),
                description: new Description( $"Rezervasyon borcu - Rezervasyon no: {reservation.ReservationNumber.Value}" ),
                lastPaymentAt: paidAmount > 0 ? new LastPaymentAt( DateTime.Now ) : null );

            customerBalanceRepository.Add( customerBalance );

            if ( paidAmount > 0 )
            {
                var paymentHistory = PaymentHistory.Create(
                    customerId: new IdentityId( request.CustomerId ),
                    sourceType: BalanceSourceType.Reservation,
                    sourceId: reservation.Id,
                    paymentTypeId: new IdentityId( request.PaymentTypeId ),
                    paymentAmount: paidAmount,
                    remainingBalance: outstandingBalance,
                    paymentDate: DateTime.Now,
                    notes: $"Rezervasyon oluşturulması sırasında yapılan ödeme - Rezervasyon no: {reservation.ReservationNumber.Value}" );

                paymentHistoryRepository.Add( paymentHistory );
            }
        }

        await unitOfWork.SaveChangesAsync( cancellationToken );
        return "Rezervasyon başarıyla oluşturuldu";
    }

    private static bool IsFaultyStatus( string? statusName )
        => string.Equals( statusName?.Trim(), CoolingRoomStatusConstants.Faulty, StringComparison.OrdinalIgnoreCase );
}
