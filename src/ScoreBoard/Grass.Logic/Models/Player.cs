namespace Grass.Logic.Models;

/// <summary>Initializes a new instance of the Player class.</summary>
/// <param name="name">Player name.</param>
public class Player( string name )
{
	/// <summary>Name of the player.</summary>
	public string Name { get; set; } = name;

	/// <summary>Current hand for the player.</summary>
	public Hand Current
	{
		get
		{
			_currentHand ??= new Hand { Player = Name, Number = Hands.Count + 1 };
			return _currentHand;
		}
	}
	private Hand? _currentHand;

	internal void ResetCurrent() => _currentHand = null;

	/// <summary>Current score for the game.</summary>
	public int Total { get; internal set; }

	/// <summary>Cards held at the end of the last hand.</summary>
	/// <returns>An empty collection is returned if no hands have been completed.</returns>
	[System.ComponentModel.EditorBrowsable( System.ComponentModel.EditorBrowsableState.Never )]
	public List<Card> GetLastHandCards()
	{
		Hand? hand = Hands.LastOrDefault();
		if( hand is null ) return [];
		return Hands.Last().Cards;
	}

	/// <summary>Gets the number of rounds in the last hand.</summary>
	/// <returns>Zero is returned if no rounds have been completed.</returns>
	[System.ComponentModel.EditorBrowsable( System.ComponentModel.EditorBrowsableState.Never )]
	public int GetLastRoundsCount()
	{
		Hand? hand = Hands.LastOrDefault();
		if( hand is null ) return 0;
		return hand.Round;
	}

	/// <inheritdoc/>
	[System.ComponentModel.EditorBrowsable( System.ComponentModel.EditorBrowsableState.Never )]
	public override string ToString() => $"{Name} game total {Total:$###,##0}";

	/// <summary>Gets the list of player hands.</summary>
	public List<Hand> Hands { get; private set; } = [];
}