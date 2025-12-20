using DeKayaServer.Application;
using DeKayaServer.Infrastructure;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.RateLimiting;
using Scalar.AspNetCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
/// <summary>
/// Rate limiter şu işe yarar: Belirli bir zaman diliminde (örneğin, saniye, dakika) bir kullanıcı veya istemcinin yapabileceği istek sayısını sınırlamak için kullanılır.
/// Böylece, aşırı yüklenmeyi önler, hizmet reddi saldırılarını azaltır ve genel performansı artırır.
/// EN: The rate limiter does the following: It is used to limit the number of requests a user or client can make within a specific time frame (for example, second, minute).
/// And so, it prevents overload, reduces denial of service attacks, and improves overall performance.
builder.Services.AddRateLimiter(cfr =>
{
    cfr.AddFixedWindowLimiter("fixed", options =>
    {
        options.PermitLimit = 100;
        options.QueueLimit = 100;
        options.Window = TimeSpan.FromSeconds(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

builder.Services
    .AddControllers()
    .AddOData(options =>
    options.Select()
            .Filter()
            .Count()
            .Expand()
            .OrderBy()
            .SetMaxTop(null));

builder.Services.AddCors();
builder.Services.AddOpenApi();

var app = builder.Build();
app.MapOpenApi();
app.MapScalarApiReference();
app.UseHttpsRedirection();
app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetPreflightMaxAge(TimeSpan.FromMinutes(10)));

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().RequireRateLimiting("fixed");
app.Run();
