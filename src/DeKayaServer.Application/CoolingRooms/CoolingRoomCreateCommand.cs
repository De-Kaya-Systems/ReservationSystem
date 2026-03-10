using DeKayaServer.Application.Behaviors;
using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.CoolingRooms;
using DeKayaServer.Domain.CoolingRooms.ValueObjects;
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
    bool IsActive,
    Guid StatusId ) : IRequest<Result<string>>;

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
    IUnitOfWork unitOfWork ) : IRequestHandler<CoolingRoomCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle( CoolingRoomCreateCommand request, CancellationToken cancellationToken )
    {
        var nameExists = await coolingRoomRepository.AnyAsync( x => x.RoomName.Value == request.RoomName, cancellationToken );
        if ( nameExists )
        {
            return Result<string>.Failure( "Bu oda adı zaten kullanılıyor" );
        }

        RoomName roomName = new( request.RoomName );
        DailyPrice dailyPrice = new( request.DailyPrice );
        Shelf shelf = new( request.Shelf );
        IdentityId statusId = new( request.StatusId );

        CoolingRoom coolingRoom = new( roomName, dailyPrice, shelf, statusId, request.IsActive );
        coolingRoomRepository.Add( coolingRoom );
        await unitOfWork.SaveChangesAsync( cancellationToken );
        return "Oda başarıyla oluşturuldu";
    }
}