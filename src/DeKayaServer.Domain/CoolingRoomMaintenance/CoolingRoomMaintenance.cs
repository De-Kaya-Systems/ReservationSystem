using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.CoolingRoomMaintenance.ValueObjects;

namespace DeKayaServer.Domain.CoolingRoomMaintenance;

public class CoolingRoomMaintenance : Entity
{
    public CoolingRoomMaintenance() { }

    public CoolingRoomMaintenance(
        Description description,
        MaintenanceDateStart maintenanceDateStart,
        MaintenanceDateEnd maintenanceDateEnd,
        IdentityId statusId )
    {
        SetDescription( description );
        SetMaintenanceDateStart( maintenanceDateStart );
        SetMaintenanceDateEnd( maintenanceDateEnd );
        SetStatusId( statusId );
    }

    public Description Description { get; private set; } = default!;
    public MaintenanceDateStart MaintenanceDateStart { get; private set; } = default!;
    public MaintenanceDateEnd MaintenanceDateEnd { get; private set; } = default!;
    public IdentityId StatusId { get; private set; } = default!;

    #region Behaviors

    public void SetDescription( Description description )
    {
        Description = description;
    }

    public void SetMaintenanceDateStart( MaintenanceDateStart maintenanceDateStart )
    {
        MaintenanceDateStart = maintenanceDateStart;
    }

    public void SetMaintenanceDateEnd( MaintenanceDateEnd maintenanceDateEnd )
    {
        MaintenanceDateEnd = maintenanceDateEnd;
    }

    public void SetStatusId( IdentityId statusId )
    {
        StatusId = statusId;
    }
    #endregion
}
