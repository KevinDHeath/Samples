using System.ComponentModel;
using Grass.Logic.Models;
namespace Grass.Logic;

/// <summary>Class to provide the ability to pass cards.</summary>
[EditorBrowsable( EditorBrowsableState.Never )]
public abstract class PassCardHandler : IDisposable
{
	/// <summary>Default event handler for paranoia played.</summary>
	/// <param name="sender">Sender object.</param>
	/// <param name="e">Property changed event arguments.</param>
	protected virtual void OnParanoiaPlayed( object? sender, PropertyChangedEventArgs e )
	{
		if( sender is Game game && e.PropertyName is not null )
		{
			if( e.PropertyName == nameof( Game.ParanoiaPlayer ) && game.ParanoiaPlayer is not null )
			{
				foreach( Player player in game.Players )
				{
					// Assume the paranoia card is already played
					Card worst = Decision.GetWorstCard( player.Current.Cards );
					game.AddCardToPass( player, worst );
				}
			}
		}
	}

	/// <inheritdoc/>
	public virtual void Dispose() => GC.SuppressFinalize( this );
}