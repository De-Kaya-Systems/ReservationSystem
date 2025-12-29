using DeKayaServer.BlazorApp.Components;
using DeKayaServer.BlazorApp.Http;
using DeKayaServer.BlazorApp.Interfaces;
using DeKayaServer.BlazorApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents();

//All Services (DI)
builder.Services.AddScoped<IBreadcrumbService, BreadcrumbService>();

// Burada DekayaSystemUrlRewriteHandler'? HTTP istemcisine ekliyoruz ve böylece DeKayaSystem API'sine yap?lan istekler do?ru ?ekilde yönlendiriliyor.
// EN: Here, we add DekayaSystemUrlRewriteHandler to the HTTP client, so that requests made to the DeKayaSystem API are routed correctly.
builder.Services.AddTransient<DekayaSystemUrlRewriteHandler>();
builder.Services.AddHttpClient("DeKayaSystem")
    .AddHttpMessageHandler<DekayaSystemUrlRewriteHandler>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>();

app.Run();
