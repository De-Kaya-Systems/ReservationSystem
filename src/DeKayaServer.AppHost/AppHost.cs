using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<DeKayaServer_WebAPI>("webApi");
builder.AddProject<DeKayaServer_BlazorApp>("blazorApp");

builder.Build().Run();
