using Grass.Logic.Models;

namespace Grass.Score.Services;

internal class GameService
{
	private Game game = default!;

	internal GameService()
	{ }

	internal Game GetGame( List<Player> players )
	{
		game = Game.Setup( players, auto: true );
		return game;
	}
}