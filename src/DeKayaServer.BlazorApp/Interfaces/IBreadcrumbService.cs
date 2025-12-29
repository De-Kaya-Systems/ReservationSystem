using DeKayaServer.BlazorApp.ViewModels;

namespace DeKayaServer.BlazorApp.Interfaces;

public interface IBreadcrumbService
{
    List<BreadcrumbItemViewModel> GetBreadcrumbItems(string currentPath);
}