using DeKayaServer.Application.Behaviors;
using DeKayaServer.Domain.Reservations;
using DeKayaServer.Domain.Reservations.Enum;
using DeKayaServer.Domain.Reservations.ValueObjects;
using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Reservations;

[Permission( "reservation:edit" )]
public sealed record ReservationMarkAsDeliveredCommand(
    Guid Id,
    DateTime DeliveredAt ) : IRequest<Result<string>>;

public sealed class ReservationMarkAsDeliveredCommandValidator : AbstractValidator<ReservationMarkAsDeliveredCommand>
{
    public ReservationMarkAsDeliveredCommandValidator()
    {
        RuleFor( x => x.Id )
            .NotEmpty()
            .WithMessage( "Geçerli bir rezervasyon seçin!" );

        RuleFor( x => x.DeliveredAt )
            .NotEmpty()
            .WithMessage( "Gerçek teslim tarihi zorunludur." )
            .Must( deliveredAt => deliveredAt <= DateTime.Now )
            .WithMessage( "Gerçek teslim tarihi gelecek bir tarih olamaz." );
    }
}

internal sealed class ReservationMarkAsDeliveredCommandHandler(
    IReservationRepository reservationRepository,
    IUnitOfWork unitOfWork ) : IRequestHandler<ReservationMarkAsDeliveredCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        ReservationMarkAsDeliveredCommand request,
        CancellationToken cancellationToken )
    {
        var reservation = await reservationRepository.FirstOrDefaultAsync(
            x => x.Id == request.Id,
            cancellationToken );

        if ( reservation is null )
        {
            return Result<string>.Failure( "Rezervasyon bulunamadı!" );
        }

        if ( reservation.Status == ReservationStatus.Completed )
        {
            return Result<string>.Failure( "Tamamlanmış rezervasyon için teslim kaydı değiştirilemez." );
        }

        if ( reservation.Status == ReservationStatus.Cancelled )
        {
            return Result<string>.Failure( "İptal edilmiş rezervasyon için teslim kaydı girilemez." );
        }

        if ( reservation.Status == ReservationStatus.DeliveredToCustomer )
        {
            return Result<string>.Failure( "Bu rezervasyon zaten müşteriye teslim edilmiş olarak işaretlenmiş." );
        }

        if ( reservation.Status != ReservationStatus.Scheduled )
        {
            return Result<string>.Failure( "Bu rezervasyon durumu teslim kaydı için uygun değil." );
        }

        reservation.MarkAsDelivered( new DeliveredAt( request.DeliveredAt ) );
        reservationRepository.Update( reservation );

        await unitOfWork.SaveChangesAsync( cancellationToken );

        return "Rezervasyon müşteriye teslim edildi olarak işaretlendi.";
    }
}
