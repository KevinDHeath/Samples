﻿namespace ClassLibrary.Models;

public class MovieReview
{
	public int Id { get; set; }

	public int MovieId { get; set; }

	public string Title { get; set; } = string.Empty;

	public string Review { get; set; } = string.Empty;
}