using DeKayaServer.Application.Services;
using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.Role;
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
        var configuration = server.GetRequiredService<IConfiguration>();

        // sys_admin role'ü zaten seed'lenmiş olmalı
        Role? role = await roleRepository.FirstOrDefaultAsync( p => p.Name.Value == "sys_admin" );

        if ( role is null )
        {
            return;  // Role yok, user oluşturma
        }

        // Secrets.json'dan admin user bilgilerini oku
        // EN: Read admin user information from secrets.json
        string? adminUserName = configuration[ "AdminUser:UserName" ];
        string? adminEmail = configuration[ "AdminUser:Email" ];
        string? adminPassword = configuration[ "AdminUser:Password" ];

        // Kontrol: Bilgiler var mı?
        if ( string.IsNullOrEmpty( adminUserName ) ||
             string.IsNullOrEmpty( adminEmail ) ||
             string.IsNullOrEmpty( adminPassword ) )
        {
            return;  // Secrets eksik, user oluşturma
        }

        // Kontrol: User zaten var mı?
        if ( await userRepository.AnyAsync( p => p.UserName.Value == adminUserName ) )
        {
            return;  // User zaten var
        }

        // Admin user oluştur
        // EN: Create admin user
        FirstName firstName = new( "Erdem" );
        LastName lastName = new( "Kaya" );
        Email email = new( adminEmail );
        UserName userName = new( adminUserName );
        Password password = new( adminPassword );
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

    public static async Task RemovePermissionsFromRolesAsync( this WebApplication app )
    {
        using var scope = app.Services.CreateScope();
        var permissionCleanerService = scope.ServiceProvider.GetRequiredService<PermissionCleanerService>();
        await permissionCleanerService.RemovePermissionsFromRolesAsync();
    }
}
