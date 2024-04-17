var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.OpentelemetryAspire_ApiService>("apiservice");

builder.AddProject<Projects.OpentelemetryAspire_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();