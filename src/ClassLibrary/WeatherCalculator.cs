namespace ClassLibrary;

internal static class WeatherCalculator
{
	/// <summary>Determines the season based on a month.</summary>
	/// <param name="date">Date containing the month.</param>
	/// <returns>The northern hemisphere season.</returns>
	public static string DetermineSeason( DateOnly date )
	{
		return date.Month switch
		{
			3 or 4 or 5 => "Spring",
			6 or 7 or 8 => "Summer",
			9 or 10 or 11 => "Autumn",
			_ => "Winter"
		};
	}

	/// <summary>Adds a number of days to today.</summary>
	/// <param name="numberOfDays">Number of days.</param>
	/// <returns>Date based on the number of days provided.</returns>
	internal static DateOnly AddDaysToToday( int numberOfDays )
	{
		return DateOnly.FromDateTime( DateTime.Now.AddDays( numberOfDays ) );
	}
}