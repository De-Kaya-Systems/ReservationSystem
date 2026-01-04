using DeKayaServer.Infrastructure.Context;
using DeKayaServer.Infrastructure.Options;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Scrutor;

namespace DeKayaServer.Infrastructure;

public static class ServiceRegistrar
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.ConfigureOptions<JwtSetupOptions>();
        services.AddAuthentication().AddJwtBearer();
        services.AddAuthorization();

        //Email Service FluentEmailSmtp kullaniyoruz. Burada SMTP ayarlarini yapabilirsiniz.Test ortami icin smtp4dev kullaniyoruz.Daha sonra Azure Email Service kullanacak.
        //EN : We use FluentEmailSmtp for Email Service. You can configure SMTP settings here. We use smtp4dev for the test environment. Later, it will use Azure Email Service. 
        services.Configure<MailSettingOptions>(configuration.GetSection("MailSettings"));
        using var scoped = services.BuildServiceProvider().CreateScope();
        var mailSettings = scoped.ServiceProvider.GetRequiredService<IOptions<MailSettingOptions>>();
        if (string.IsNullOrEmpty(mailSettings.Value.UserId))
        {
            services.AddFluentEmail(mailSettings.Value.Email)
                .AddSmtpSender(
                    mailSettings.Value.Smtp,
                    mailSettings.Value.Port);
        }
        else
        {
            services.AddFluentEmail(mailSettings.Value.Email)
                .AddSmtpSender(
                    mailSettings.Value.Smtp,
                    mailSettings.Value.Port,
                    mailSettings.Value.UserId,
                    mailSettings.Value.Password);
        }

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
