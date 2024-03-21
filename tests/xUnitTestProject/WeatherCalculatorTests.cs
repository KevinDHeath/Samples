global using FluentAssertions;
using ClassLibrary;

namespace xUnitTestProject;

public class WeatherCalculatorTests
{
	[Theory]
	[InlineData( 3, "Spring" )]
	[InlineData( 4, "Spring" )]
	[InlineData( 5, "Spring" )]
	[InlineData( 6, "Summer" )]
	[InlineData( 7, "Summer" )]
	[InlineData( 8, "Summer" )]
	[InlineData( 9, "Autumn" )]
	[InlineData( 10, "Autumn" )]
	[InlineData( 11, "Autumn" )]
	[InlineData( 12, "Winter" )]
	[InlineData( 1, "Winter" )]
	[InlineData( 2, "Winter" )]
	public void DetermineSeason_ShouldReturn_ExpectedSeason( int month, string expectedSeason )
	{
		// Arrange
		DateOnly date = new( DateTime.Now.Year, month, 1 );

		// Act
		string result = WeatherCalculator.DetermineSeason( date );

		result.Should().Be( expectedSeason );
	}

	[Fact]
	public void DetermineSeason_ShouldReturnWinter_ForLeapYear()
	{
		// Arrange
		DateOnly date = new( 2024, 2, 29 );

		// Act
		string result = WeatherCalculator.DetermineSeason( date );

		// Assert
		result.Should().Be( "Winter" );
	}
}