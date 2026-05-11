using DeKayaServer.Application.Behaviors;
using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.Constants;
using DeKayaServer.Domain.CoolingRoomMaintenance;
using DeKayaServer.Domain.CoolingRoomMaintenance.ValueObjects;
using DeKayaServer.Domain.CoolingRooms;
using DeKayaServer.Domain.CoolingRoomStatus;
using DeKayaServer.Domain.CustomerBalance;
using DeKayaServer.Domain.CustomerBalance.Enum;
using DeKayaServer.Domain.CustomerBalance.ValueObjects;
using DeKayaServer.Domain.PaymentHistory;
using DeKayaServer.Domain.Reservations;
using DeKayaServer.Domain.Reservations.Enum;
using DeKayaServer.Domain.Reservations.ValueObjects;
using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Reservations;

[Permission( "reservation:edit" )]
public sealed record ReservationCompleteCommand(
    Guid Id,
    DateTime DeliveredAt,
    DateTime PickedUpAt,
    bool PaymentReceived,
    Guid? PaymentTypeId,
    decimal PaymentAmount,
    bool HasFault,
    string? FaultDescription,
    string? Note ) : IRequest<Result<string>>;

public sealed class ReservationCompleteCommandValidator : AbstractValidator<ReservationCompleteCommand>
{
    public ReservationCompleteCommandValidator()
    {
        RuleFor( x => x.Id )
            .NotEmpty()
            .WithMessage( "Geçerli bir rezervasyon seçin!" );

        RuleFor( x => x.DeliveredAt )
            .NotEmpty()
            .WithMessage( "Gerçek teslim tarihi zorunludur." );

        RuleFor( x => x.PickedUpAt )
            .NotEmpty()
            .WithMessage( "Gerçek geri alım tarihi zorunludur." );

        RuleFor( x => x )
            .Must( x => x.PickedUpAt >= x.DeliveredAt )
            .WithMessage( "Geri alım tarihi teslim tarihinden önce olamaz." );

        RuleFor( x => x.PaymentAmount )
            .GreaterThanOrEqualTo( 0 )
            .WithMessage( "Ödeme tutarı negatif olamaz." );

        RuleFor( x => x.PaymentTypeId )
            .NotEmpty()
            .WithMessage( "Ödeme tipi seçilmelidir." );

        RuleFor( x => x.PaymentAmount )
            .GreaterThanOrEqualTo( 0 )
            .WithMessage( "Ödeme tutarı negatif olamaz." );

        When( x => x.PaymentReceived, () =>
        {
            RuleFor( x => x.PaymentAmount )
                .GreaterThan( 0 )
                .WithMessage( "Ödeme alındıysa ödeme tutarı 0'dan büyük olmalıdır." );
        } );

        When( x => !x.PaymentReceived, () =>
        {
            RuleFor( x => x.PaymentAmount )
                .Equal( 0 )
                .WithMessage( "Ödeme alınmadıysa ödeme tutarı 0 olmalıdır." );
        } );

        When( x => x.HasFault, () =>
        {
            RuleFor( x => x.FaultDescription )
                .NotEmpty()
                .WithMessage( "Arıza varsa arıza açıklaması girilmelidir." );
        } );
    }
}

internal sealed class ReservationCompleteCommandHandler(
    IReservationRepository reservationRepository,
    ICoolingRoomRepository coolingRoomRepository,
    ICoolingRoomStatusRepository coolingRoomStatusRepository,
    ICoolingRoomMaintenanceRepository coolingRoomMaintenanceRepository,
    ICustomerBalanceRepository customerBalanceRepository,
    IPaymentHistoryRepository paymentHistoryRepository,
    IUnitOfWork unitOfWork ) : IRequestHandler<ReservationCompleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        ReservationCompleteCommand request,
        CancellationToken cancellationToken )
    {
        var reservation = await reservationRepository.FirstOrDefaultAsync( x => x.Id == request.Id, cancellationToken );

        if ( reservation is null )
        {
            return Result<string>.Failure( "Rezervasyon bulunamadı!" );
        }

        if ( reservation.Status == ReservationStatus.Completed )
        {
            return Result<string>.Failure( "Bu rezervasyon zaten tamamlanmış." );
        }

        if ( reservation.Status == ReservationStatus.Cancelled )
        {
            return Result<string>.Failure( "İptal edilmiş rezervasyon tamamlanamaz." );
        }

        var coolingRoom = await coolingRoomRepository.FirstOrDefaultAsync( x => x.Id == reservation.CoolingRoomId.Value, cancellationToken );

        if ( coolingRoom is null )
        {
            return Result<string>.Failure( "Soğuk oda bulunamadı!" );
        }

        var totalAmount = reservation.ReservationTotalAmount.Value;
        var existingPaidAmount = reservation.PaidAtReservation.Value;
        var paymentAmount = request.PaymentReceived ? request.PaymentAmount : 0;
        var newPaidAmount = existingPaidAmount + paymentAmount;

        if ( newPaidAmount > totalAmount )
        {
            return Result<string>.Failure( "Alınan ödeme toplam rezervasyon tutarından büyük olamaz." );
        }

        var newOutstandingAmount = totalAmount - newPaidAmount;
        reservation.SetPaidAtReservation( new PaidAtReservation( newPaidAmount ) );

        var deliverdAt = new DeliveredAt( request.DeliveredAt );
        var pickedUpAt = new PickedUpAt( request.PickedUpAt );

        reservation.CompleteReservation( deliverdAt, pickedUpAt );

        reservation.SetNote(
            string.IsNullOrWhiteSpace( request.Note )
                ? reservation.Note
                : new Note( request.Note ) );

        reservationRepository.Update( reservation );

        var balanceResult = await UpdateCustomerBalanceAsync(
            reservation,
            request,
            totalAmount,
            newPaidAmount,
            newOutstandingAmount,
            paymentAmount,
            cancellationToken );

        if ( !balanceResult.IsSuccessful )
        {
            return balanceResult;
        }

        var roomStatusResult = await UpdateCoolingRoomStatusAsync(
            coolingRoom,
            request,
            cancellationToken );

        if ( !roomStatusResult.IsSuccessful )
        {
            return roomStatusResult;
        }

        await unitOfWork.SaveChangesAsync( cancellationToken );

        return "Rezervasyon başarıyla tamamlandı.";
    }

    private async Task<Result<string>> UpdateCustomerBalanceAsync(
        Reservation reservation,
        ReservationCompleteCommand request,
        decimal totalAmount,
        decimal newPaidAmount,
        decimal newOutstandingAmount,
        decimal paymentAmount,
        CancellationToken cancellationToken )
    {
        var customerBalance = await customerBalanceRepository.FirstOrDefaultAsync(
            x => x.SourceType == BalanceSourceType.Reservation
                && x.SourceId == reservation.Id,
            cancellationToken );

        if ( customerBalance is not null )
        {
            customerBalance.SetAmounts(
                totalAmount: new TotalAmount( totalAmount ),
                paidAmount: new PaidAmount( newPaidAmount ),
                outstandingAmount: new OutstandingAmount( newOutstandingAmount ) );

            customerBalance.SetDescription(
                new Domain.CustomerBalance.ValueObjects.Description( $"Rezervasyon borcu - RezervasyonNo: {reservation.ReservationNumber.Value}" ) );

            if ( request.PaymentReceived && request.PaymentTypeId is not null )
            {
                customerBalance.SetPaymentType( new IdentityId( request.PaymentTypeId.Value ) );
                customerBalance.SetLastPaymentAt( new LastPaymentAt( DateTime.Now ) );
            }

            customerBalanceRepository.Update( customerBalance );
        }
        else if ( newOutstandingAmount > 0 )
        {
            if ( request.PaymentTypeId is null )
            {
                return Result<string>.Failure( "Rezervasyon bakiyesi bulunamadı. Kalan borç için ödeme tipi gerekli." );
            }

            customerBalance = new CustomerBalance(
                customerId: reservation.CustomerId,
                sourceType: BalanceSourceType.Reservation,
                sourceId: reservation.Id,
                paymentTypeId: new IdentityId( request.PaymentTypeId.Value ),
                totalAmount: new TotalAmount( totalAmount ),
                outstandingAmount: new OutstandingAmount( newOutstandingAmount ),
                paidAmount: new PaidAmount( newPaidAmount ),
                description: new Domain.CustomerBalance.ValueObjects.Description( $"Rezervasyon borcu - Rezervasyon no: {reservation.ReservationNumber.Value}" ),
                lastPaymentAt: request.PaymentReceived ? new LastPaymentAt( DateTime.Now ) : null );

            customerBalanceRepository.Add( customerBalance );
        }

        if ( request.PaymentReceived && paymentAmount > 0 )
        {
            if ( request.PaymentTypeId is null )
            {
                return Result<string>.Failure( "Ödeme tipi seçilmelidir." );
            }

            var paymentHistory = PaymentHistory.Create(
                customerId: reservation.CustomerId,
                sourceType: BalanceSourceType.Reservation,
                sourceId: reservation.Id,
                paymentTypeId: new IdentityId( request.PaymentTypeId.Value ),
                paymentAmount: paymentAmount,
                remainingBalance: newOutstandingAmount,
                paymentDate: DateTime.Now,
                notes: $"Rezervasyon tamamlanırken alınan ödeme - Rezervasyon no: {reservation.ReservationNumber.Value}" );

            paymentHistoryRepository.Add( paymentHistory );
        }

        return Result<string>.Succeed( string.Empty );
    }

    private async Task<Result<string>> UpdateCoolingRoomStatusAsync(
        CoolingRoom coolingRoom,
        ReservationCompleteCommand request,
        CancellationToken cancellationToken )
    {
        var targetStatusName = request.HasFault
            ? CoolingRoomStatusConstants.Faulty
            : CoolingRoomStatusConstants.Available;

        var targetStatus = await coolingRoomStatusRepository.FirstOrDefaultAsync(
            x => x.StatusName.Value == targetStatusName,
            cancellationToken );

        if ( targetStatus is null )
        {
            return Result<string>.Failure( $"Oda durumu bulunamadı: {targetStatusName}" );
        }

        coolingRoom.SetRoomStatusId( targetStatus.Id );

        if ( request.HasFault )
        {
            var maintenance = new CoolingRoomMaintenance(
                coolingRoomId: coolingRoom.Id,
                description: new Domain.CoolingRoomMaintenance.ValueObjects.Description( request.FaultDescription! ),
                maintenanceDateStart: new MaintenanceDateStart( DateOnly.FromDateTime( request.PickedUpAt ) ),
                maintenanceDateEnd: new MaintenanceDateEnd( DateOnly.FromDateTime( request.PickedUpAt ) ),
                statusId: targetStatus.Id );

            await coolingRoomMaintenanceRepository.AddAsync( maintenance, cancellationToken );
            coolingRoom.SetMaintenanceId( maintenance.Id );
        }
        else
        {
            coolingRoom.SetMaintenanceId( null );
        }

        coolingRoomRepository.Update( coolingRoom );

        return Result<string>.Succeed( string.Empty );
    }
}