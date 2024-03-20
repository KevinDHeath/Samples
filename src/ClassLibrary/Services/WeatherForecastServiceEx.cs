using ClassLibrary.Models;

namespace ClassLibrary.Services;

public class WeatherForecastServiceEx : WeatherForecastService
{
	public override IEnumerable<WeatherForecast> Get()
	{
		return Get( 15 );
	}
}