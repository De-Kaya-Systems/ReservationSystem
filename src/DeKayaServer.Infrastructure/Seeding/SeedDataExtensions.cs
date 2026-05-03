using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.CoolingRoomStatus;
using DeKayaServer.Domain.CoolingRoomStatus.ValueObjects;
using DeKayaServer.Domain.PaymentTypes;
using DeKayaServer.Domain.Role;
using DeKayaServer.Domain.Shared;
using GenericRepository;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DeKayaServer.Infrastructure.Seeding;

/// <summary>
/// Seeding işlemleri için extension method'lar.
/// EN: Extension methods for seeding operations.
/// </summary>
public static class SeedDataExtensions
{
    /// <summary>
    /// Tüm default verileri database'e ekler (varsa eklememez).
    /// EN: Adds all default data to the database (doesn't add if it already exists).
    /// </summary>
    public static async Task SeedDefaultDataAsync( this WebApplication app )
    {
        // Yeni bir scope oluştur (bağımsız bir DI container örneği)
        // EN: Create a new scope (independent DI container instance)
        using var scope = app.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        // Repository ve UnitOfWork'u al
        // EN: Get the repository and unit of work
        var coolingRoomStatusRepository = serviceProvider.GetRequiredService<ICoolingRoomStatusRepository>();
        var paymentTypesRepository = serviceProvider.GetRequiredService<IPaymentTypesRepository>();
        var roleRepository = serviceProvider.GetRequiredService<IRoleRepository>();
        var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();

        // Seeding işlemlerini çalıştır
        // EN: Run seeding operations
        await SeedCoolingRoomStatusesAsync( coolingRoomStatusRepository, unitOfWork );
        await SeedPaymentTypesAsync( paymentTypesRepository, unitOfWork );
        await SeedRolesAsync( roleRepository, unitOfWork );

    }

    /// <summary>
    /// CoolingRoomStatus tablosuna default verileri ekler.
    /// EN: Adds default data to CoolingRoomStatus table.
    /// </summary>
    private static async Task SeedCoolingRoomStatusesAsync(
        ICoolingRoomStatusRepository repository,
        IUnitOfWork unitOfWork )
    {
        // KONTROL: Verileri zaten ekledik mi?
        // EN: CHECK: Did we already add the data?
        var hasData = await repository.AnyAsync( x => true );

        // Eğer veri varsa, işlemi sonlandır (idempotent = güvenli)
        // EN: If data exists, end the operation (idempotent = safe)
        if ( hasData )
        {
            return;
        }

        // OLUŞTUR: Default status verilerini oluştur
        // EN: CREATE: Create default status data
        var statuses = new List<CoolingRoomStatus>
        {
            new CoolingRoomStatus(new StatusName("Uygun"), true),
            new CoolingRoomStatus(new StatusName("Rezerve"), true),
            new CoolingRoomStatus(new StatusName("Booked"), true),
            new CoolingRoomStatus(new StatusName("Arıza/Bakım"), false)
        };

        // EKLE: Repository'ye ekle
        // EN: ADD: Add to repository
        repository.AddRange( statuses );

        // KAYDET: Database'e kaydet
        // EN: SAVE: Save to database
        await unitOfWork.SaveChangesAsync();
    }

    private static async Task SeedPaymentTypesAsync(
        IPaymentTypesRepository paymentTypesRepository,
        IUnitOfWork unitOfWork )
    {
        // KONTROL: Verileri zaten ekledik mi?
        // EN: CHECK: Did we already add the data?
        var hasData = await paymentTypesRepository.AnyAsync( x => true );

        // Eğer veri varsa, işlemi sonlandır (idempotent = güvenli)
        // EN: If data exists, end the operation (idempotent = safe)
        if ( hasData )
        {
            return;
        }

        // OLUŞTUR: Default ödeme türü verilerini oluştur
        // EN: CREATE: Create default payment type data
        var paymentTypes = new List<PaymentTypes>
        {
            new(new PaymentType("Havale/EFT"), true),
            new(new PaymentType("Kredi Kartı"), true),
            new(new PaymentType("Nakit"), true),
            new(new PaymentType("Çek"), true),
            new(new PaymentType("NoPay"), true)
        };

        // EKLE: Repository'ye ekle
        // EN: ADD: Add to repository
        paymentTypesRepository.AddRange( paymentTypes );

        // KAYDET: Database'e kaydet
        // EN: SAVE: Save to database
        await unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Role tablosuna default verileri ekler.
    /// EN: Adds default data to Role table.
    /// </summary>
    private static async Task SeedRolesAsync(
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork)
    {
        // KONTROL: Verileri zaten ekledik mi?
        // EN: CHECK: Did we already add the data?
        var hasData = await roleRepository.AnyAsync(x => true);

        // Eğer veri varsa, işlemi sonlandır (idempotent = güvenli)
        // EN: If data exists, end the operation (idempotent = safe)
        if (hasData)
        {
            return;
        }

        // OLUŞTUR: Default role verilerini oluştur
        // EN: CREATE: Create default role data
        var roles = new List<Role>
        {
            new(new Name("sys_admin"), true),
            new(new Name("Admin"), true),
            new(new Name("Moderator"), true),
            new(new Name("User"), true)
        };

        // EKLE: Repository'ye ekle
        // EN: ADD: Add to repository
        roleRepository.AddRange(roles);

        // KAYDET: Database'e kaydet
        // EN: SAVE: Save to database
        await unitOfWork.SaveChangesAsync();
    }
}
