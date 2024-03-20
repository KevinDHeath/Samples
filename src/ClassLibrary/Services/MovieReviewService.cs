namespace ClassLibrary.Services;

public class MovieReviewService : BaseService, IMovieReviewService
{
	private static List<Movie> Movies = [];

	public Movie? GetMovie( int id ) => Movies.SingleOrDefault( m => m.Id == id );

	public async Task<List<Movie>> GetMovies()
	{
		if( Movies.Count == 0 ) { Movies = await GetMovieDataAsync(); }
		return Movies;
	}
}