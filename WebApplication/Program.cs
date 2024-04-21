using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using WebApplication;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Otel Logging
// Register OpenTelemetry ILogger
// Nuget OpenTelemetry
builder.Logging.AddOpenTelemetry(loggingOptions =>
{
    var resourceBuilder = ResourceBuilder.CreateDefault()
        .AddService(DiagnosticsConfig.ServiceName).AddAttributes([
            new("Environment", builder.Environment.EnvironmentName),
            new("Application", "WebApplication"),
            new("Version", "1.0.0")
        ]);
    // Add resource attributes that will be added to all metrics and traces
    loggingOptions.SetResourceBuilder(resourceBuilder);

    loggingOptions
        // OpenTelemetry Exporter to backend
        // Nuget OpenTelemetry.Exporter.OpenTelemetryProtocol
        .AddOtlpExporter(exporterOptions => { exporterOptions.Endpoint = new Uri("http://localhost:4317"); });
});

// Otel Metrics + traces
// Register OpenTelemetry Metrics
// Nuget OpenTelemetry.Extensions.Hosting
builder.Services.AddOpenTelemetry()
    // Add resources that will be added to all metrics and traces
    // Ex. pod name, server, version, datacenter region, ...
    .ConfigureResource(resourceBuilder => resourceBuilder.AddService(DiagnosticsConfig.ServiceName)
        .AddAttributes([
        new("Environment", builder.Environment.EnvironmentName),
        new("Application", "WebApplication"),
        new("Version", "1.0.0")
    ]))
    .WithTracing(tracing =>
    {
        tracing
            // Nuget OpenTelemetry.Instrumentation.AspNetCore
            .AddAspNetCoreInstrumentation()
            // Nuget OpenTelemetry.Instrumentation.Http
            .AddHttpClientInstrumentation()
            // OpenTelemetry Exporter to backend
            // Nuget OpenTelemetry.Exporter.OpenTelemetryProtocol
            .AddOtlpExporter(exporterOptions => { exporterOptions.Endpoint = new Uri("http://localhost:4317"); });
    })
    .WithMetrics(metrics =>
    {
        metrics
            // Nuget OpenTelemetry.Instrumentation.AspNetCore
            .AddAspNetCoreInstrumentation()
            // Nuget OpenTelemetry.Instrumentation.Http
            .AddHttpClientInstrumentation()
            // Custom app meter
            .AddMeter(DiagnosticsConfig.Meter.Name)
            // OpenTelemetry Exporter to backend
            // Nuget OpenTelemetry.Exporter.OpenTelemetryProtocol
            .AddOtlpExporter();
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast/{location}", (string location) =>
    {
        // Increment the counter with the location in the request
        DiagnosticsConfig.RequestsCounter.Add(1, [new KeyValuePair<string, object?>("Location", location)]);
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}