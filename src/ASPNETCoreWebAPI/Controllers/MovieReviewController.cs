using Microsoft.AspNetCore.Mvc;

namespace ASPNETCoreWebAPI.Controllers;

[ApiController]
[Route( "[controller]" )]
public class MovieReviewController( ILogger<MovieReviewController> logger,
	IMovieReviewService movieReviewService ) : ControllerBase
{
	private readonly ILogger<MovieReviewController> _logger = logger;
	private readonly IMovieReviewService _movieReviewService = movieReviewService;

	[HttpGet( Name = "GetMovieReviews" )]
	public IEnumerable<Movie> Get()
	{
		return _movieReviewService.GetMovies().Result;
	}
}