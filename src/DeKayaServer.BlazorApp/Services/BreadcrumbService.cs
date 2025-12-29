using DeKayaServer.BlazorApp.Components.Layout.NavMenu.Data;
using DeKayaServer.BlazorApp.Components.Layout.NavMenu.Model;
using DeKayaServer.BlazorApp.Interfaces;
using DeKayaServer.BlazorApp.ViewModels;

namespace DeKayaServer.BlazorApp.Services;

public class BreadcrumbService : IBreadcrumbService
{
    public List<BreadcrumbItemViewModel> GetBreadcrumbItems(string currentPath)
    {
        var breadcrumbs = new List<BreadcrumbItemViewModel>
        {
            new() { Title = "Dashboard", Url = "/", Icon = "bi-house" }
        };

        if (currentPath == "/" || string.IsNullOrEmpty(currentPath))
        {
            return breadcrumbs;
        }

        var menuItem = FindMenuItem(NavigationData.Items, currentPath);

        if (menuItem != null)
        {
            BuildBreadcrumbPath(menuItem, breadcrumbs, NavigationData.Items);
        }
        else
        {
            BuildFromPath(currentPath, breadcrumbs);
        }
        return breadcrumbs;
    }

    private NavigationItem? FindMenuItem(List<NavigationItem> items, string path)
    {
        foreach (var item in items)
        {
            if (item.Url == path)
            {
                return item;
            }
            if (item.Children.Count > 0)
            {
                var found = FindMenuItem(item.Children, path);
                if (found != null)
                {
                    return found;
                }
            }
        }
        return null;
    }

    private void BuildBreadcrumbPath(NavigationItem current, List<BreadcrumbItemViewModel> breadcrumbs, List<NavigationItem> allItems)
    {
        var parent = FindParent(allItems, current);

        if (parent != null)
        {
            breadcrumbs.Add(new BreadcrumbItemViewModel
            {
                Title = parent.Title,
                Url = parent.Url,
                Icon = parent.Icon,
            });
        }

        breadcrumbs.Add(new BreadcrumbItemViewModel
        {
            Title = current.Title,
            Url = current.Url,
            IsActive = true
        });
    }

    private NavigationItem? FindParent(List<NavigationItem> items, NavigationItem target)
    {
        foreach (var item in items)
        {
            if (item.Children.Any(c => c.Url == target.Url))
                return item;

            if (item.Children.Count > 0)
            {
                var found = FindParent(item.Children, target);
                if (found != null)
                    return found;
            }
        }
        return null;
    }

    private void BuildFromPath(string path, List<BreadcrumbItemViewModel> breadcrumbs)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var currentPath = "";

        for (int i = 0; i < segments.Length; i++)
        {
            currentPath += "/" + segments[i];
            var title = FormatTitle(segments[i]);

            breadcrumbs.Add(new BreadcrumbItemViewModel
            {
                Title = title,
                Url = currentPath,
                IsActive = i == segments.Length - 1
            });
        }
    }

    private string FormatTitle(string segment)
    {
        return segment
            .Replace("-", " ")
            .Replace("_", " ")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(word => char.ToUpper(word[0]) + word.Substring(1))
            .Aggregate((a, b) => a + " " + b);
    }
}
