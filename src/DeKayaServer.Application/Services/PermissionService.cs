using DeKayaServer.Application.Behaviors;
using System.Reflection;

namespace DeKayaServer.Application.Services;

public sealed class PermissionService
{
    public List<string> GetAll()
    {
        //HasSet varsa ekle yoksa ekleme yapmaz
        //EN: If there is a HasSet, add it, otherwise do not add it.
        var permissions = new HashSet<string>( PermissionConstants.DefaultPermissions );

        //Assembly yazarak mevcut katmanda işlem yapıyoruzç
        //EN: By writing Assembly, we are working in the current layer.
        var assembly = Assembly.GetExecutingAssembly();

        //Mevcut katmandaki tüm type'ları al
        //EN: Get all types in the current layer
        var types = assembly.GetTypes();

        // Tüm type'ları dolaş ve PermissionAttribute olanları bul
        foreach ( var type in types )
        {
            var permissionAttributes = type.GetCustomAttribute<PermissionAttribute>();

            if ( permissionAttributes is not null && !string.IsNullOrEmpty( permissionAttributes.Permission ) )
            {
                permissions.Add( permissionAttributes.Permission );
            }
        }

        return permissions.ToList();
    }

    public static class PermissionConstants
    {
        //View'ler sayfa linkleri icin. Sadece blazor app da
        //EN: Views are for page links. Only in Blazor app.
        public static readonly string DashboardView = "dashboard:view";
        public static readonly string RoleView = "role:view";
        public static readonly string SeyyarRoomView = "seyyar-room:view";
        public static readonly string SeyyarRoomMaintenance = "seyyar-room:maintenance";
        public static readonly string UserView = "user:view";

        public static readonly IReadOnlyCollection<string> DefaultPermissions =
        [
            DashboardView,
            RoleView,
            SeyyarRoomView,
            SeyyarRoomMaintenance,
            UserView
        ];
    }
}
