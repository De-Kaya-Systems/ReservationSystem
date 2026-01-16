
using DeKayaServer.Domain.LoginTokens;
using GenericRepository;
using Microsoft.EntityFrameworkCore;

namespace DeKayaServer.WebAPI.Helper;

public class TokenDatabaseCleaner(
   IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var scope = serviceScopeFactory.CreateScope();
        var server = scope.ServiceProvider;

        var loginTokenService = server.GetRequiredService<ILoginTokenRepository>();
        var unitOfWork = server.GetRequiredService<IUnitOfWork>();

        //Pasif olan ve süresi 1 haftadan fazla olan tokenları bul ve sil.
        //EN: Find and delete tokens that are inactive and have expired for more than 1 week.
        var now = DateTimeOffset.Now;
        var oneWeekAgo = now.AddDays(-7);
        var activList = await loginTokenService
            .Where(x => x.IsActive.Value == false && x.ExpiresDate.Value <= oneWeekAgo)
            .ToListAsync(stoppingToken);

        if (activList.Count > 0)
        {
            loginTokenService.DeleteRange(activList);
            await unitOfWork.SaveChangesAsync(stoppingToken);
        }

        await Task.Delay(TimeSpan.FromDays(1));
    }
}