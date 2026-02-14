using DeKayaServer.Application.Behaviors;
using System.Reflection;

namespace DeKayaServer.Application.Services;

public sealed class PermissionService
{
    public List<string> GetAll()
    {
        //HasSet varsa ekle yoksa ekleme yapmaz
        // EN: If there is a HasSet, add it, otherwise do not add it.
        var permissions = new HashSet<string>();

        //Assembly yazarak mevcut katmanda işlem yapıyoruzç
        // EN: By writing Assembly, we are working in the current layer.
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
}
