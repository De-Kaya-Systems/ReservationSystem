using DeKayaServer.Infrastructure.Context;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace DeKayaServer.Infrastructure;

public static class ServiceRegistrar
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        // Infrastructure Services Registration
        // Burasi altyapi katmanina ait servislerin kaydedildigi yerdir.Ornegin: services.AddTransient<IYourService, YourServiceImplementation>();
        //EN : This is where services related to the infrastructure layer are registered.Exemple: services.AddTransient<IYourService, YourServiceImplementation>();

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            string connectionString = configuration.GetConnectionString("SqlServer")!;
            options.UseSqlServer(connectionString);
        });

        /// <summary>
        /// Burada Scrutor kullanarak tum servisleri otomatik olarak kaydediyoruz. Dependency Injection icin kolaylik saglar.
        /// FromAssemblies ile bu assemblydeki tum siniflari tarar. AddClasses ile public olmayan siniflari da dahil ederiz.
        /// UsingRegistrationStrategy ile zaten kayitli olanlari atlariz. AsImplementedInterfaces ile siniflari implement ettikleri arayuzler olarak kaydederiz.
        /// Class ismi ve interface ismi ayni olursa bu yontem otomatik olarak eslestirme yapar.
        /// 
        /// EN : Here we use Scrutor to automatically register all services. It provides ease for Dependency Injection.
        /// FromAssemblies scans all classes in this assembly. With AddClasses we include non-public classes as well.
        /// UsingRegistrationStrategy skips those that are already registered. AsImplementedInterfaces registers classes as the interfaces they implement.
        /// Class name and interface name match, this method automatically matches them.
        /// </summary>
        /// 
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.Scan(action => action
        .FromAssemblies(typeof(ServiceRegistrar).Assembly)
        .AddClasses(publicOnly: false)
        .UsingRegistrationStrategy(RegistrationStrategy.Skip)
        .AsImplementedInterfaces()
        .WithScopedLifetime());

        return services;
    }
}
