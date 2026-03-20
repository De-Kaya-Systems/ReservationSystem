using DeKayaServer.BlazorApp.Components;
using DeKayaServer.BlazorApp.Http;
using DeKayaServer.BlazorApp.Http.TokenProcess;
using DeKayaServer.BlazorApp.Interfaces;
using DeKayaServer.BlazorApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder( args );

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped( sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri( navigationManager.BaseUri ) };
} );

builder.Services.AddScoped<ProtectedLocalStorage>();

builder.Services.AddScoped<AccessTokenStoreService>();
builder.Services.AddScoped<IAccessTokenStoreService>( sp =>
    new CachedAccessTokenStoreService( sp.GetRequiredService<AccessTokenStoreService>() ) );

builder.Services.AddScoped<CurrentAccessToken>();

builder.Services.AddScoped<TokenAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>( sp =>
    sp.GetRequiredService<TokenAuthenticationStateProvider>() );

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

// Circuit -> HttpClient handler bridge (robust)
builder.Services.AddSingleton<CircuitServicesAccessor>();
builder.Services.AddScoped<CircuitIdProvider>();

builder.Services.TryAddEnumerable( ServiceDescriptor.Scoped<CircuitHandler, CircuitServicesAccessorCircuitHandler>() );
builder.Services.TryAddEnumerable( ServiceDescriptor.Scoped<CircuitHandler, CircuitIdCircuitHandler>() );

//All Services (DI)
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<IBreadcrumbService, BreadcrumbService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IApiResultPresenter, ToastApiResultPresenter>();
builder.Services.AddScoped<IForceLogoutService, ForceLogoutService>();
builder.Services.AddScoped<ApiExecutor>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<INotificationPanelService, NotificationPanelService>();
builder.Services.AddScoped<ICoolingRoomService, CoolingRoomService>();

builder.Services.AddScoped<IAuthProbeService, AuthProbeService>();

builder.Services.AddTransient<AuthHeaderHandler>();

builder.Services.AddHttpClient<IApiClient, ApiClient>( client =>
{
    client.BaseAddress = new Uri( builder.Configuration[ "ApiSettings:BaseUrl" ]! );
} ).AddHttpMessageHandler<AuthHeaderHandler>();

var app = builder.Build();

app.MapDefaultEndpoints();

if ( !app.Environment.IsDevelopment() )
{
    app.UseExceptionHandler( "/Error", createScopeForErrors: true );
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute( "/not-found" );
app.UseHttpsRedirection();

app.UseAntiforgery();
app.UseStaticFiles();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();