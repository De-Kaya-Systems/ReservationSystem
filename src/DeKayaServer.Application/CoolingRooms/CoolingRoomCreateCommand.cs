using DeKayaServer.Application.Behaviors;
using DeKayaServer.Domain.CoolingRooms;
using DeKayaServer.Domain.CoolingRooms.ValueObjects;
using DeKayaServer.Domain.CoolingRoomStatus;
using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.CoolingRooms;

[Permission( "coolingroom:create" )]
public sealed record class CoolingRoomCreateCommand(
    string RoomName,
    decimal DailyPrice,
    bool Shelf,
    bool IsActive ) : IRequest<Result<string>>;

public sealed class CoolingRoomCreateCommandValidator : AbstractValidator<CoolingRoomCreateCommand>
{
    public CoolingRoomCreateCommandValidator()
    {
        RuleFor( x => x.RoomName )
            .NotEmpty()
            .WithMessage( "Geçerli bir oda adı girin" );
        RuleFor( x => x.DailyPrice )
            .GreaterThan( 0 )
            .WithMessage( "Günlük fiyat sıfırdan büyük olmalıdır" );
    }
}

internal sealed class CoolingRoomCreateCommandHandler(
    ICoolingRoomRepository coolingRoomRepository,
    ICoolingRoomStatusRepository coolingRoomStatusRepository,
    IUnitOfWork unitOfWork ) : IRequestHandler<CoolingRoomCreateCommand, Result<string>>
{
    public const string StatusName = "Uygun";
    public async Task<Result<string>> Handle( CoolingRoomCreateCommand request, CancellationToken cancellationToken )
    {
        var nameExists = await coolingRoomRepository.AnyAsync( x => x.RoomName.Value == request.RoomName, cancellationToken );
        if ( nameExists )
        {
            return Result<string>.Failure( "Bu oda adı zaten kullanılıyor" );
        }

        var status = await coolingRoomStatusRepository.FirstOrDefaultAsync( x => x.StatusName.Value == StatusName, cancellationToken );

        if ( status is null )
        {
            return Result<string>.Failure( $"'{StatusName}' durumunu bulunamadı. Lütfen önce bu durumu oluşturun." );
        }

        RoomName roomName = new( request.RoomName );
        DailyPrice dailyPrice = new( request.DailyPrice );
        Shelf shelf = new( request.Shelf );

        CoolingRoom coolingRoom = new( roomName, dailyPrice, shelf, status.Id, request.IsActive );
        coolingRoomRepository.Add( coolingRoom );
        await unitOfWork.SaveChangesAsync( cancellationToken );
        return "Oda başarıyla oluşturuldu";
    }
}