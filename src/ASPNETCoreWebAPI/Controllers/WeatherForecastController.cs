using Microsoft.AspNetCore.Mvc;

namespace ASPNETCoreWebAPI.Controllers;

[ApiController]
[Route( "[controller]" )]
public class WeatherForecastController( ILogger<WeatherForecastController> logger,
	IWeatherForecastService weatherForecastService ) : ControllerBase
{
	private readonly ILogger<WeatherForecastController> _logger = logger;
	private readonly IWeatherForecastService _weatherForecastService = weatherForecastService;

	[HttpGet( Name = "GetWeatherForecast" )]
	public IEnumerable<WeatherForecast> Get()
	{
		return _weatherForecastService.Get();
	}
}