using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<DeKayaServer_WebAPI>("webApi");

builder.Build().Run();
