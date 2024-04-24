using System.Diagnostics.Metrics;

namespace WebApplication;

public static class DiagnosticsConfig
{
    public static string ServiceName = "DemoWebApplication";
    // Create a custom meter
    public static Meter Meter = new (ServiceName);
    
    // Add instruments on the meter
    public static Counter<int> RequestsCounter = Meter.CreateCounter<int>("requests");
    public static Histogram<long> RequestsHistogram = Meter.CreateHistogram<long>("request-time");
}