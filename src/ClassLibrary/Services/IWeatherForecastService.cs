﻿using ClassLibrary.Models;

namespace ClassLibrary.Services;

public interface IWeatherForecastService
{
	IEnumerable<WeatherForecast> Get();
}