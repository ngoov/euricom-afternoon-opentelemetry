namespace WebApplication.Services;

public class WeatherService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public WeatherService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    public async Task GetWeatherAsync(string location)
    {
        var httpClient = _httpClientFactory.CreateClient();
        // The http client instrumentation (.AddHttpClientInstrumentation) will add a span for it
        var responseMessage = await httpClient.GetAsync("https://euri.com/");
    }
}