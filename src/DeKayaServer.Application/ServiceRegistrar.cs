using DeKayaServer.Application.Behaviors;
using DeKayaServer.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;

namespace DeKayaServer.Application;

public static class ServiceRegistrar
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<PermissionService>();
        // Application Services Registration
        // Burasi uygulama katmanina ait servislerin kaydedildigi yerdir.Ornegin: services.AddTransient<IYourService, YourServiceImplementation>();
        //EN : This is where services related to the application layer are registered.Exemple: services.AddTransient<IYourService, YourServiceImplementation>();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ServiceRegistrar).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(PermissionBehavior<,>));
        });
        services.AddValidatorsFromAssembly(typeof(ServiceRegistrar).Assembly);

        return services;
    }
}
