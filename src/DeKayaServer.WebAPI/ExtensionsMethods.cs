using DeKayaServer.Application.Services;
using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.Role;
using DeKayaServer.Domain.Shared;
using DeKayaServer.Domain.Users;
using DeKayaServer.Domain.Users.ValueObjects;
using GenericRepository;

namespace DeKayaServer.WebAPI;

public static class ExtensionsMethods
{
    public static async Task CreateFirstUser( this WebApplication app )
    {
        using var scope = app.Services.CreateScope();
        var server = scope.ServiceProvider;

        var userRepository = server.GetRequiredService<IUserRepository>();
        var roleRepository = server.GetRequiredService<IRoleRepository>();
        var unitOfWork = server.GetRequiredService<IUnitOfWork>();

        Role? role = await roleRepository.FirstOrDefaultAsync( p => p.Name.Value == "sys_admin" );

        if ( role is null )
        {
            Name name = new( "sys_admin" );
            role = new Role( name, true );
            roleRepository.Add( role );
        }

        if ( !( await userRepository.AnyAsync( p => p.UserName.Value == "admin" ) ) )
        {
            FirstName firstName = new( "Erdem" );
            LastName lastName = new( "Kaya" );
            Email email = new( "erdem.kaya@de-kaya.com" );
            UserName userName = new( "admin" );
            Password password = new( "Admin2026!" );
            IdentityId roleId = role.Id;

            var user = new User(
                firstName,
                lastName,
                email,
                userName,
                password,
                roleId );

            userRepository.Add( user );
            await unitOfWork.SaveChangesAsync();
        }
    }

    public static async Task RemovePermissionsFromRolesAsync( this WebApplication app )
    {
        using var scope = app.Services.CreateScope();
        var permissionCleanerService = scope.ServiceProvider.GetRequiredService<PermissionCleanerService>();
        await permissionCleanerService.RemovePermissionsFromRolesAsync();
    }
}
