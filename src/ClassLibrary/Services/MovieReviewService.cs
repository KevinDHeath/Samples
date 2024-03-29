namespace ClassLibrary.Services;

public class MovieReviewService : BaseService, IMovieReviewService
{
	private static List<Movie> Movies = [];

	public async Task<Movie?> GetMovie( int id )
	{
		if( Movies.Count == 0 ) { Movies = await GetMovies(); }
		return Movies.SingleOrDefault( m => m.Id == id );
	}

	public async Task<List<Movie>> GetMovies()
	{
		if( Movies.Count == 0 ) { Movies = await GetMovieDataAsync(); }
		return Movies;
	}
}