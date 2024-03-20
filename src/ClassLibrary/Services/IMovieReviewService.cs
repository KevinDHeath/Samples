namespace ClassLibrary.Services;

public interface IMovieReviewService
{
	Task<List<Movie>> GetMovies();

	Movie? GetMovie( int id );
}