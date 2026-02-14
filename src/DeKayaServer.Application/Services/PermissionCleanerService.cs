using DeKayaServer.Domain.Role;
using GenericRepository;
using Microsoft.EntityFrameworkCore;

namespace DeKayaServer.Application.Services;

public sealed class PermissionCleanerService(
    IRoleRepository roleRepository,
    PermissionService permissionService,
    IUnitOfWork unitOfWork )
{
    public async Task RemovePermissionsFromRolesAsync( CancellationToken cancellationToken = default )
    {
        //Permission Attribute kaldirilirsa bunu databaseden temizlemek icin kullanilir
        //EN: This is used to clean up the database if the Permission Attribute is removed.

        var currentPermissions = permissionService.GetAll();
        var roles = await roleRepository.GetAllWithTracking().ToListAsync( cancellationToken );

        foreach ( var role in roles )
        {
            var currentPermissionForRole = role.Permissions.Select( s => s.Value ).ToList();
            var filteredPermissions = currentPermissionForRole.Where( p => currentPermissions.Contains( p ) ).ToList();

            var permissions = filteredPermissions.Select( s => new Permission( s ) ).ToList();
            role.SetPermissions( permissions );
        }
        roleRepository.UpdateRange( roles );
        await unitOfWork.SaveChangesAsync( cancellationToken );
    }
}