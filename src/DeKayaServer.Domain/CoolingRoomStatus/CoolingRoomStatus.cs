using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.CoolingRoomStatus.ValueObjects;

namespace DeKayaServer.Domain.CoolingRoomStatus;

public class CoolingRoomStatus : Entity
{
    public CoolingRoomStatus() { }
    public CoolingRoomStatus(
        StatusName statusName,
        bool isActive )
    {
        SetStatusName( statusName );
        SetStatus( isActive );
    }

    public StatusName StatusName { get; private set; } = default!;

    #region Behaviors
    public void SetStatusName( StatusName statusName )
    {
        StatusName = statusName;
    }

    #endregion
}
