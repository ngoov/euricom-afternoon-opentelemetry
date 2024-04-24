Sample app for an afternoon given at Euricom.

## How to run

### Getting Started WebApplication
1. Start the docker containers
```shell
docker compose up -d
```
2. Start the web application
```shell
cd WebApplication
dotnet run
```
3. Watch the different visualizers
- [Seq http://localhost:8080/](http://localhost:8080/)
- [Jaeger http://localhost:16686/](http://localhost:16686/)
- [Zipkin http://localhost:9411/](http://localhost:9411/)
- [Prometheus http://localhost:9090](http://localhost:9090/)
- [Grafana http://localhost:3000/](http://localhost:3000/)
- [Aspire dashboard http://0.0.0.0:18888/](http://localhost:18888/)

## Resources
- [Youtube Practical OTel in .Net 8 (Mark Thwaites NDC 2024)](https://www.youtube.com/watch?v=WzZI_IT6gYo&t=1368s)
- [Microsoft docs](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-with-otel)
- [opentelemetry-dotnet github](https://github.com/open-telemetry/opentelemetry-dotnet)
- [Getting started blogpost .NET 8](https://www.mytechramblings.com/posts/getting-started-with-opentelemetry-metrics-and-dotnet-part-2/)
- [Meziantou blogpost](https://www.meziantou.net/monitoring-a-dotnet-application-using-opentelemetry.htm)

## Workshops/exercises
- [Workshop OpenTelemetry in .NET](https://github.com/lennartpost/workshop-opentelemetry)
- [Grafana labs workshop](https://github.com/grafana/opentelemetry-workshop)
- [Microsoft diagnostics tutorial](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/#net-core-diagnostics-tutorials)