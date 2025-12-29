using DeKayaServer.BlazorApp.Components.Layout.NavMenu.Model;

namespace DeKayaServer.BlazorApp.Components.Layout.NavMenu.Data;

public static class NavigationData
{
    public static readonly List<NavigationItem> Items = [
        new()
        {
            Title = "Dashboard",
            Url = "/",
            Icon = "bi-speedometer2"
        },
        new()
        {
            Title = "Seyyar Odalar",
            Icon = "bi-snow",
            Children =
            [
                new() { Title = "Tüm Odalar", Url = "/cooling-rooms" },
                new() { Title = "Yeni Araç Ekle", Url = "/cooling-rooms/new" },
                new() { Title = "Bakım Takibi", Url = "/cooling-rooms/maintenance" },
            ]
        }
    ];
}
