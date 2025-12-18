using DeKayaServer.Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;

namespace DeKayaServer.Application;

public static class RegistrarService
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Application Services Registration
        // Burasi uygulama katmanina ait servislerin kaydedildigi yerdir.Ornegin: services.AddTransient<IYourService, YourServiceImplementation>();
        //EN : This is where services related to the application layer are registered.Exemple: services.AddTransient<IYourService, YourServiceImplementation>();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(RegistrarService).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(PermissionBehavior<,>));
        });
        services.AddValidatorsFromAssembly(typeof(RegistrarService).Assembly);

        return services;
    }
}
