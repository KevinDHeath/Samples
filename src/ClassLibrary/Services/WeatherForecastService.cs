namespace ClassLibrary.Services;

public class WeatherForecastService : BaseService, IWeatherForecastService
{
	public virtual IEnumerable<WeatherForecast> Get()
	{
		return Get( 5 );
	}

	private static readonly string[] Summaries =
	[
		"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
	];

	internal static IEnumerable<WeatherForecast> Get( int max )
	{
		return Enumerable.Range( 1, max ).Select( index =>
		{
			DateOnly date = WeatherCalculator.AddDaysToToday( index );
			return new WeatherForecast
			{
				Date = date,
				TemperatureC = Random.Shared.Next( -20, 55 ),
				Summary = Summaries[Random.Shared.Next( Summaries.Length )],
				Season = WeatherCalculator.DetermineSeason( date )
			};
		}).ToArray();
	}
}