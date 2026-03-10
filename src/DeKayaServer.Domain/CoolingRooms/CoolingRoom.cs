using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.CoolingRooms.ValueObjects;

namespace DeKayaServer.Domain.CoolingRooms;

public sealed class CoolingRoom : Entity
{
    public CoolingRoom() { }

    public CoolingRoom(
        RoomName name,
        DailyPrice dailyPrice,
        Shelf shelf,
        IdentityId roomStatusId,
        bool isActive
        )
    {
        SetName( name );
        SetDailyPrice( dailyPrice );
        SetShelf( shelf );
        SetRoomStatusId( roomStatusId );
        SetStatus( isActive );
    }

    public RoomName RoomName { get; private set; } = default!;
    public DailyPrice DailyPrice { get; private set; } = default!;
    public Shelf Shelf { get; private set; } = default!;
    public IdentityId RoomStatusId { get; private set; } = default!;
    public IdentityId? MaintenanceId { get; private set; }

    #region Behaviors

    public void SetName( RoomName roomName )
    {
        RoomName = roomName;
    }

    public void SetDailyPrice( DailyPrice dailyPrice )
    {
        DailyPrice = dailyPrice;
    }

    public void SetShelf( Shelf shelf )
    {
        Shelf = shelf;
    }

    public void SetRoomStatusId( IdentityId roomStatusId )
    {
        RoomStatusId = roomStatusId;
    }

    public void SetMaintenanceId( IdentityId? maintenanceId )
    {
        MaintenanceId = maintenanceId;
    }

    #endregion
}