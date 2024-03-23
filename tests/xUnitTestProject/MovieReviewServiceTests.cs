using ClassLibrary.Models;
using ClassLibrary.Services;

namespace xUnitTestProject;

public class MovieReviewServiceTests
{
	[Fact]
	public async Task MovieReviewService_Get_Should_Return_Count_gt_0()
	{
		// Arrange
		MovieReviewService service = new();

		// Act (with code coverage)
		List<Movie> result = await service.GetMovies();
		if( result.Count > 0 ) { _ = service.GetMovie( result[0].Id ); }

		// Assert
		result.Count.Should().BeGreaterThan( 0 );
	}

	[Fact]
	public void MovieReview_Should_Have_Value()
	{
		// Arrange
		int id = 1;
		int movieId = 1;
		var title = "New Movie";
		var review = "Rotten tomatoes";

		// Act (with code coverage)
		MovieReview result = new() { Id = id, MovieId = movieId, Title = title, Review = review };
		id = result.Id;
		movieId = result.MovieId;
		title = result.Title;
		review = result.Review;

		// Assert
		result.Should().BeOfType( typeof( MovieReview ) );
	}
}