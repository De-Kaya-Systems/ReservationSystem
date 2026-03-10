using DeKayaServer.Application.Behaviors;
using DeKayaServer.Domain.CoolingRoomMaintenance;
using DeKayaServer.Domain.CoolingRoomMaintenance.ValueObjects;
using DeKayaServer.Domain.CoolingRooms;
using DeKayaServer.Domain.CoolingRooms.ValueObjects;
using DeKayaServer.Domain.CoolingRoomStatus;
using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.CoolingRooms;

[Permission( "coolingroom:edit" )]
public sealed record CoolingRoomUpdateCommand(
    Guid Id,
    string RoomName,
    decimal DailyPrice,
    bool Shelf,
    bool IsActive,
    CoolingRoomMaintenanceUpdateModel? Maintenance ) : IRequest<Result<string>>;

public sealed record CoolingRoomMaintenanceUpdateModel(
    bool Remove,
    string? Description,
    DateTime MaintenanceDateStart,
    DateTime MaintenanceDateEnd );

public sealed class CoolingRoomUpdateCommandValidator : AbstractValidator<CoolingRoomUpdateCommand>
{
    public CoolingRoomUpdateCommandValidator()
    {
        RuleFor( x => x.RoomName )
            .NotEmpty()
            .WithMessage( "Geçerli bir oda adı girin" );

        RuleFor( x => x.DailyPrice )
            .GreaterThan( 0 )
            .WithMessage( "Günlük fiyat sıfırdan büyük olmalıdır" );

        When( x => x.Maintenance is not null && x.Maintenance.Remove == false, () =>
        {
            RuleFor( x => x.Maintenance!.Description )
                .NotEmpty()
                .WithMessage( "Geçerli bir bakım/açıklama girin" );

            RuleFor( x => x.Maintenance )
                .Must( m => m!.MaintenanceDateStart <= m.MaintenanceDateEnd )
                .WithMessage( "Bakım başlangıç tarihi bitiş tarihinden büyük olamaz" );
        } );
    }
}

internal sealed class CoolingRoomUpdateCommandHandler(
    ICoolingRoomRepository coolingRoomRepository,
    ICoolingRoomMaintenanceRepository coolingRoomMaintenanceRepository,
    ICoolingRoomStatusRepository coolingRoomStatusRepository,
    IUnitOfWork unitOfWork ) : IRequestHandler<CoolingRoomUpdateCommand, Result<string>>
{
    private const string UygunStatusName = "Uygun";
    private const string ArizaBakimStatusName = "Arıza/Bakım";

    public async Task<Result<string>> Handle( CoolingRoomUpdateCommand request, CancellationToken cancellationToken )
    {
        var coolingRoom = await coolingRoomRepository.FirstOrDefaultAsync( x => x.Id == request.Id, cancellationToken );
        if ( coolingRoom is null )
        {
            return Result<string>.Failure( "Oda bulunamadı" );
        }

        if ( coolingRoom.RoomName.Value != request.RoomName )
        {
            var nameExists = await coolingRoomRepository.AnyAsync( x => x.RoomName.Value == request.RoomName, cancellationToken );
            if ( nameExists )
            {
                return Result<string>.Failure( "Bu oda adı zaten kullanılıyor" );
            }
        }

        coolingRoom.SetName( new RoomName( request.RoomName ) );
        coolingRoom.SetDailyPrice( new DailyPrice( request.DailyPrice ) );
        coolingRoom.SetShelf( new Shelf( request.Shelf ) );
        coolingRoom.SetStatus( request.IsActive );

        if ( request.Maintenance is not null )
        {
            if ( request.Maintenance.Remove )
            {
                if ( coolingRoom.MaintenanceId is not null )
                {
                    var maintenance = await coolingRoomMaintenanceRepository
                        .FirstOrDefaultAsync( x => x.Id == coolingRoom.MaintenanceId.Value, cancellationToken );

                    if ( maintenance is not null )
                    {
                        maintenance.Delete();
                        coolingRoomMaintenanceRepository.Update( maintenance );
                    }

                    coolingRoom.SetMaintenanceId( null );
                }

                var uygunStatus = await coolingRoomStatusRepository
                    .FirstOrDefaultAsync( x => x.StatusName.Value == UygunStatusName, cancellationToken );

                if ( uygunStatus is null )
                {
                    return Result<string>.Failure( $"Oda durumu bulunamadı: {UygunStatusName}" );
                }

                coolingRoom.SetRoomStatusId( uygunStatus.Id );
            }
            else
            {
                var arizaBakimStatus = await coolingRoomStatusRepository
                    .FirstOrDefaultAsync( x => x.StatusName.Value == ArizaBakimStatusName, cancellationToken );

                if ( arizaBakimStatus is null )
                {
                    return Result<string>.Failure( $"Oda durumu bulunamadı: {ArizaBakimStatusName}" );
                }

                var description = new Description( request.Maintenance.Description! );
                var maintenanceDateStart = new MaintenanceDateStart( DateOnly.FromDateTime( request.Maintenance.MaintenanceDateStart ) );
                var maintenanceDateEnd = new MaintenanceDateEnd( DateOnly.FromDateTime( request.Maintenance.MaintenanceDateEnd ) );

                var maintenanceStatusId = arizaBakimStatus.Id;

                if ( coolingRoom.MaintenanceId is null )
                {
                    var maintenance = new CoolingRoomMaintenance( description, maintenanceDateStart, maintenanceDateEnd, maintenanceStatusId );
                    await coolingRoomMaintenanceRepository.AddAsync( maintenance, cancellationToken );
                    coolingRoom.SetMaintenanceId( maintenance.Id );
                }
                else
                {
                    var maintenance = await coolingRoomMaintenanceRepository
                        .FirstOrDefaultAsync( x => x.Id == coolingRoom.MaintenanceId.Value, cancellationToken );

                    if ( maintenance is null )
                    {
                        return Result<string>.Failure( "Bakım kaydı bulunamadı" );
                    }

                    maintenance.SetDescription( description );
                    maintenance.SetMaintenanceDateStart( maintenanceDateStart );
                    maintenance.SetMaintenanceDateEnd( maintenanceDateEnd );
                    maintenance.SetStatusId( maintenanceStatusId );

                    coolingRoomMaintenanceRepository.Update( maintenance );
                }

                coolingRoom.SetRoomStatusId( arizaBakimStatus.Id );
            }
        }

        coolingRoomRepository.Update( coolingRoom );
        await unitOfWork.SaveChangesAsync( cancellationToken );
        return "Oda başarıyla güncellendi";
    }
}