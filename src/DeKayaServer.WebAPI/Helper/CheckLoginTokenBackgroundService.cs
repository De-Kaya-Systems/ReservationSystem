
using DeKayaServer.Domain.LoginTokens;
using GenericRepository;
using Microsoft.EntityFrameworkCore;

namespace DeKayaServer.WebAPI.Helper;

public class CheckLoginTokenBackgroundService(
   IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var scope = serviceScopeFactory.CreateScope();
        var server = scope.ServiceProvider;

        var loginTokenService = server.GetRequiredService<ILoginTokenRepository>();
        var unitOfWork = server.GetRequiredService<IUnitOfWork>();

        var now = DateTimeOffset.Now;
        var activList = await loginTokenService
            .Where(x => x.IsActive.Value == true && x.ExpiresDate.Value < now)
            .ToListAsync(stoppingToken);

        foreach (var item in activList)
        {
            item.SetIsActive(new(false));
        }

        if (activList.Any())
        {
            loginTokenService.UpdateRange(activList);
            await unitOfWork.SaveChangesAsync(stoppingToken);

        }

        await Task.Delay(TimeSpan.FromDays(1));
    }
}
