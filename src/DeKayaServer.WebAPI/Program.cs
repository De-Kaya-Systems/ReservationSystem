using DeKayaServer.Application;
using DeKayaServer.Infrastructure;
using DeKayaServer.WebAPI;
using DeKayaServer.WebAPI.Controllers;
using DeKayaServer.WebAPI.Helper;
using DeKayaServer.WebAPI.Middelwares;
using DeKayaServer.WebAPI.Modules;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.RateLimiting;
using Scalar.AspNetCore;
using System.Threading.RateLimiting;
using TS.Result;

var builder = WebApplication.CreateBuilder( args );

builder.AddServiceDefaults();
builder.Services.AddApplication();
builder.Services.AddInfrastructure( builder.Configuration );
/// <summary>
/// Rate limiter şu işe yarar: Belirli bir zaman diliminde (örneğin, saniye, dakika) bir kullanıcı veya istemcinin yapabileceği istek sayısını sınırlamak için kullanılır.
/// Böylece, aşırı yüklenmeyi önler, hizmet reddi saldırılarını azaltır ve genel performansı artırır.
/// EN: The rate limiter does the following: It is used to limit the number of requests a user or client can make within a specific time frame (for example, second, minute).
/// And so, it prevents overload, reduces denial of service attacks, and improves overall performance.
builder.Services.AddRateLimiter( cfr =>
{
    cfr.AddFixedWindowLimiter( "fixed", options =>
    {
        options.PermitLimit = 100;
        options.QueueLimit = 100;
        options.Window = TimeSpan.FromSeconds( 1 );
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    } );
    cfr.AddFixedWindowLimiter( "login-fixed", options =>
    {
        options.PermitLimit = 5;
        options.QueueLimit = 1;
        options.Window = TimeSpan.FromMinutes( 1 );
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    } );
    cfr.AddFixedWindowLimiter( "forgot-password-fixed", options =>
    {
        options.PermitLimit = 2;
        options.Window = TimeSpan.FromMinutes( 3 );
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    } );
    cfr.AddFixedWindowLimiter( "reset-password-fixed", options =>
    {
        options.PermitLimit = 3;
        options.Window = TimeSpan.FromMinutes( 1 );
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    } );
    cfr.AddFixedWindowLimiter( "check-forgot-password-code-fixed", options =>
    {
        options.PermitLimit = 2;
        options.Window = TimeSpan.FromMinutes( 1 );
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    } );
} );

builder.Services
    .AddControllers()
    .AddOData( options =>
    options.Select()
            .Filter()
            .Count()
            .Expand()
            .OrderBy()
            .SetMaxTop( null )
            .AddRouteComponents( "odata", MainODataController.GetEdmModel() ) );

builder.Services.AddCors();
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<ExceptionHandler>().AddProblemDetails();
//Response compression kullaniyoruz cunku veriyi sıkıştırarak ağ üzerinden iletimini optimize eder.
//En: We use response compression because it optimizes the transmission of data over the network by compressing it.
builder.Services.AddResponseCompression( options =>
{
    options.EnableForHttps = true;
} );
builder.Services.AddTransient<CheckTokenMiddleware>();
builder.Services.AddHostedService<CheckLoginTokenBackgroundService>();
builder.Services.AddHostedService<TokenDatabaseCleaner>();

var app = builder.Build();
app.MapOpenApi();
app.MapScalarApiReference();
app.UseHttpsRedirection();
app.UseCors( policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetPreflightMaxAge( TimeSpan.FromMinutes( 10 ) ) );

app.UseResponseCompression();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<CheckTokenMiddleware>();
app.UseExceptionHandler();
app.UseRateLimiter();
app.MapControllers().RequireRateLimiting( "fixed" ).RequireAuthorization();
app.MapAuth();
app.MapRole();
app.MapPermission();

// root endpoint anonymous kalsın
app.MapGet( "/", () => Results.Ok( Result<string>.Succeed( "OK" ) ) );

// authorized probe endpoint
app.MapGet( "/auth/probe", () => Results.Ok( Result<string>.Succeed( "Authorized OK" ) ) )
   .RequireAuthorization();

app.MapDefaultEndpoints();
//await app.CreateFirstUser();
await app.RemovePermissionsFromRolesAsync();
app.Run();
