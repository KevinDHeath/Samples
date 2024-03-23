using ClassLibrary.Services;

namespace xUnitTestProject;

public class WeatherForecastServiceTests
{
	[Fact]
	public void WeatherForecastService_Get_Should_Return_5()
	{
		// Arrange
		WeatherForecastService service = new();

		// Act (with code coverage)
		var result = service.Get().ToList();
		if( result.Count > 0 ) { _ = result[0].TemperatureF; }

		// Assert
		result.Should().HaveCount( 5 );
	}

	[Fact]
	public void WeatherForecastServiceEx_Get_Should_Return_15()
	{
		// Arrange
		WeatherForecastServiceEx service = new();

		// Act
		var result = service.Get();

		// Assert
		result.Should().HaveCount( 15 );
	}
}