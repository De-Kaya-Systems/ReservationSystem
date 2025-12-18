using DeKayaServer.Application.Services;
using System.Reflection;
using TS.MediatR;

namespace DeKayaServer.Application.Behaviors;

public sealed class PermissionBehavior<TRequest, TResponse>(
    IUserContext userContext) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default)
    {
        var attr = request.GetType().GetCustomAttribute<PermissionAttribute>(inherit: true);

        if (attr is null) return await next();

        var userId = userContext.GetUserId();
        //var user = await userRepository.FirstOrDefaultAsync(p => p.Id == userId, cancellationToken);
        //if (user is null)
        //{
        //    throw new ArgumentException("User not found");
        //}

        // Eğer permission string'i varsa kontrol et
        // EN: If there is a permission string, check it
        //if (!string.IsNullOrEmpty(attr.Permission))
        //{
        //    var hasPermission = user.Permissions.Any(p => p.Name == attr.Permission);
        //    if (!hasPermission)
        //    {
        //        throw new AuthorizationException($"'{attr.Permission}' you are not authorized.");
        //    }
        //}

        //// Eğer permission string'i yoksa sadece admin kontrolü yap
        //// EN: If there is no permission string, just check for admin
        //else if (!user.IsAdmin.Value)
        //{
        //    throw new AuthorizationException("Administrator privileges are required for this operation.");
        //}

        return await next();
    }
}

public sealed class PermissionAttribute : Attribute
{
    public string? Permission { get; }

    public PermissionAttribute()
    {
    }

    public PermissionAttribute(string permission)
    {
        Permission = permission;
    }
}

public sealed class AuthorizationException : Exception
{
    public AuthorizationException() : base("You do not have permission.")
    {
    }

    public AuthorizationException(string message) : base(message)
    {
    }
}
