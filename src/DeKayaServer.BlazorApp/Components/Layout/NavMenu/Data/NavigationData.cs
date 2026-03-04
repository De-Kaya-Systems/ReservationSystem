using DeKayaServer.BlazorApp.Components.Layout.NavMenu.Model;

namespace DeKayaServer.BlazorApp.Components.Layout.NavMenu.Data;

public static class NavigationData
{
    public static readonly List<NavigationItem> Items = [
        new()
        {
            Title = "Dashboard",
            Url = "/",
            Icon = "bi-speedometer2",
            Permission = "dashboard:page"
        },
        new()
        {
            Title = "Seyyar Odalar",
            Icon = "bi-snow",
            Children =
            [
                new() {
                    Title = "Tüm Odalar",
                    Url = "/cooling-rooms",
                    Permission = "seyyar-room:page"
                },
                new() {
                    Title = "Bakım Takibi",
                    Url = "/cooling-rooms/maintenance",
                    Permission = "seyyar-room:maintenance"
                },
            ]
        },
        new()
        {
            Title = "Müşteri Yönetimi",
            Url = "/customers",
            Icon = "bi bi-person-vcard",
            Permission = "customer:page"
        },
        new()
        {
            Title = "Role Yönetimi",
            Url = "/roles",
            Icon = "bi-shield-lock",
            Permission = "role:page"
        },
        new()
        {
            Title = "Kullanıcı Yönetimi",
            Url = "/users",
            Icon = "bi bi-people",
            Permission = "user:page"
        }
    ];
}
