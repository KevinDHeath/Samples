using Grass.Logic.Models;
using static System.Net.WebRequestMethods;
namespace Grass.Logic;

/// <summary>Game service.</summary>
public class GameService : PassCardHandler
{
	private readonly Game _game;
	private bool _disposed;

	/// <summary>Initializes a new instance of the <see cref="GameService"/> class.</summary>
	/// <param name="game">Game to be played.</param>
	public GameService( Game game )
	{
		_game = game;
		if( _game.Auto ) { _game.GameChanged += OnParanoiaPlayed; }
	}

	/// <summary>Play a game asynchronously.</summary>
	/// <returns><c>true</c> is returned if the game was completed successfully.</returns>
	/// <remarks><c>await Task.Run()</c> scenarios can be avoided if the created task is returned directly.</remarks>
	/// <seealso href="https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/async-scenarios">
	/// Asynchronous programming scenarios</seealso>
	public Task<bool> PlayAsync() => Task.Run( () =>
	{
		return _game.Play();
	} );

	/// <inheritdoc/>
	public override void Dispose()
	{
		if( _disposed ) { return; }
		if( _game.Auto ) { _game.GameChanged -= OnParanoiaPlayed; }
		GC.SuppressFinalize( this );
		_disposed = true;
	}
}