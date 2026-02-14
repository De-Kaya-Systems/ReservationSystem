using DeKayaServer.Application.Services;
using DeKayaServer.Domain.Users;
using DeKayaServer.Domain.Users.ValueObjects;
using GenericRepository;

namespace DeKayaServer.WebAPI;

public static class ExtensionsMethods
{
    public static async Task CreateFirstUser( this WebApplication app )
    {
        using var scope = app.Services.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        if ( !( await userRepository.AnyAsync( p => p.UserName.Value == "admin" ) ) )
        {
            FirstName firstName = new( "Erdem" );
            LastName lastName = new( "Kaya" );
            Email email = new( "erdem.kaya@de-kaya.com" );
            UserName userName = new( "admin" );
            Password password = new( "Admin2026!" );

            var user = new User( firstName, lastName, email, userName, password );

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
