var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.OpenTelemetryAspire_ApiService>("apiservice");

builder.AddProject<Projects.OpenTelemetryAspire_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();