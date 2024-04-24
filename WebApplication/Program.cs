using System.Diagnostics;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using WebApplication;
using WebApplication.Services;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .Enrich.WithProcessId()
    .WriteTo.Console()
    .CreateLogger();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddTransient<WeatherService>();

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
        .AddOtlpExporter(exporterOptions => { exporterOptions.Endpoint = new Uri("http://localhost:4317"); })
        // OpenTelemetry Exporter to backend
        // Nuget OpenTelemetry.Exporter.OpenTelemetryProtocol
        .AddConsoleExporter();
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
            // See the WeatherService where a HttpClient is created and instrumented 
            .AddHttpClientInstrumentation()
            // OpenTelemetry Exporter to backend
            // Nuget OpenTelemetry.Exporter.OpenTelemetryProtocol
            .AddOtlpExporter(exporterOptions => { exporterOptions.Endpoint = new Uri("http://localhost:4317"); })
            // OpenTelemetry Exporter to backend
            // Nuget OpenTelemetry.Exporter.OpenTelemetryProtocol
            .AddConsoleExporter();
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
            .AddOtlpExporter()
            // Nuget OpenTelemetry.Exporter.Console
            .AddConsoleExporter();
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

var activitySource = new ActivitySource("SampleActivitySource");

// ASP.NET Core starts an activity when handling a request
app.MapGet("/weatherforecast/{location}", async (string location, WeatherService weatherService) =>
    {
        // Increment the counter with the location in the request
        DiagnosticsConfig.RequestsCounter.Add(1, [new KeyValuePair<string, object?>("Location", location)]);
        
        var stopwatch = Stopwatch.StartNew();
        
        // Measure the duration in ms of requests and record it with the location in the request
        DiagnosticsConfig.RequestsHistogram.Record(stopwatch.ElapsedMilliseconds,
            tag: KeyValuePair.Create<string, object?>("location", location));
        // The sampleActivity is automatically linked to the parent activity (the one from
        // ASP.NET Core in this case).
        // You can get the current activity using Activity.Current.
        using var sampleActivity = activitySource.StartActivity("Sample", ActivityKind.Server);
        // note that "sampleActivity" can be null here if nobody listens to the events generated
        // by the "SampleActivitySource" activity source.
        sampleActivity?.AddTag("Location", location);
        sampleActivity?.AddBaggage("SampleContext", location);
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        // To add an extra span to the activity
        await weatherService.GetWeatherAsync(location);
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

Log.Logger.Information("Process ID: {ProcessId}", Environment.ProcessId);
app.Run();


record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}