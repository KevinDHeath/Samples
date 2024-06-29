namespace Grass.Logic.Models;

/// <summary>Provides the details and actions of a Game.</summary>
public class Game
{
	private List<Card> GrassStack { get; set; } = [];

	/// <summary>Number of cards left in the stack.</summary>
	public int StackCount => GrassStack.Count;

	/// <summary>List of players in the game.</summary>
	public List<Player> Players { get; private set; }

	/// <summary>Game target value.</summary>
	public int Target { get; private set; }

	/// <summary>Game date.</summary>
	public string Date { get; private set; }

	/// <summary>Winner of the game.</summary>
	/// <returns>If the game is not completed <see langword="null"/> is returned.</returns>
	public Player? Winner { get; private set; }

	/// <summary>List of cards currently in the wasted pile.</summary>
	public List<Card> WastedPile { get; private set; } = [];

	/// <summary>Initializes a new instance of the Game class.</summary>
	/// <param name="players">List of players in the game.</param>
	/// <param name="target">Target for the game. The default is $250,000</param>
	/// <param name="auto">Indicates whether to use auto-play. The default is <see langword="false"/>.
	/// <br/><b>Note:</b><i> This should only be set as </i><see langword="true"/><i> for testing purposes
	/// as it automates the decision process of which card each player will play.</i></param>
	/// <returns>An initialized game of Grass.</returns>
	public static Game Start( List<Player> players, int target = 250000, bool auto = false )
	{
		Game game = new( players, target, auto );
		return game;
	}

	/// <summary>Start a hand of the game.</summary>
	/// <returns><see langword="true"/> if the hand was successfully started.</returns>
	/// <remarks>There must be 104 cards and 2-6 players to play a game.</remarks>
	public bool StartHand()
	{
		GrassStack = CardInfo.BuildStack();
		if( WastedPile.Count > 0 ) { WastedPile.Clear(); } // Reset wasted pile
		if( GrassStack.Count != Rules.cCardTotal ) { return false; } // Check # of cards
		return Deal();
	}

	/// <summary>End a hand of the game.</summary>
	/// <returns>The player with the highest score at the end of the hand, or
	/// <see langword="null"/> if one could not be determined.</returns>
	public Player? EndHand()
	{
		// Calculate net scores
		Player? banker = GetBanker();
		foreach( Player player in Players )
		{
			player.Current.EndHand( banker );
			player.Hands.Add( player.Current );
		}

		// Recalculate the banker net score after all skims
		banker?.Current.EndHand( null );

		// Assign bonus for highest score and update players game total
		int highScore = 0;
		Player? rtn = null;
		foreach( Player player in Players )
		{
			player.Total += player.Current.NetScore;
			if( player.Current.NetScore > highScore )
			{
				highScore = player.Current.NetScore;
				rtn = player;
			}
		}
		if( rtn is not null )
		{
			rtn.Current.Bonus = Rules.cBonusAmount;
			rtn.Total += rtn.Current.Bonus;
		}

		return rtn;
	}

	/// <summary>Take the next card from the stack.</summary>
	/// <param name="hand">Player hand taking a card.</param>
	/// <param name="card">Specific card to take.</param>
	/// <returns><see langword="true"/> if the card is successfully taken
	/// from the stack and added to the players hand.</returns>
	public bool TakeCard( Hand hand, Card? card = null )
	{
		if( GrassStack.Count == 0 ) { return false; }
		card ??= GrassStack[0];
		bool rtn = GrassStack.Remove( card );
		if( rtn ) { hand.Cards.Add( card ); }

		return rtn;
	}

	/// <summary>Trade a card from one players hand to another.</summary>
	/// <param name="yourHand">Your current hand.</param>
	/// <param name="yourCard">Your card to trade.</param>
	/// <param name="otherHand">Other players current hand.</param>
	/// <param name="otherCard">Other players card to trade.</param>
	/// <returns><see langword="true"/> if the cards are successfully transfered.</returns>
	public static bool TradeCard( Hand yourHand, Card yourCard, Hand otherHand, Card otherCard )
	{
		// TODO: need to undo 1st transfer if 2nd fails
		// Could check that both cards exist in collections first
		bool ok = Card.TransferCard( otherHand.Cards, yourHand.Cards, otherCard );
		if( ok ) { ok = Card.TransferCard( yourHand.Cards, otherHand.Cards, yourCard ); }
		return ok;
	}

	/// <summary>Play a card.</summary>
	/// <param name="hand">Hand to remove the card from.</param>
	/// <param name="list">List to add the card to.</param>
	/// <param name="card">Card to remove.</param>
	/// <returns><see langword="true"/> if the card is successfully played.</returns>
	public static bool PlayCard( Hand hand, List<Card> list, Card card )
	{
		bool rtn = hand.Cards.Remove( card );
		if( rtn ) { list.Add( card ); }
		return rtn;
	}

	internal Game( List<Player> players, int target = 0, bool auto = false )
	{
		Players = players;
		Target = target;
		Auto = auto;
		Date = DateTime.Now.ToString( @"MMMM d, yyyy hh:mm" );
	}

	#region Internal Constructor and Methods

	internal bool PlayHeatOff( Hand hand, Card card, Card? fine )
	{
		if( !Rules.CanPlayCard( hand, CardInfo.cHeatOff ) ) { return false; }

		bool rtn = PlayCard( hand, hand.HasslePile, card );
		if( rtn ) // Pay any required fine and remove heat on
		{
			if( fine is not null && hand.StashPile.Remove( fine ) ) { WastedPile.Add( fine ); }
			Rules.RemoveHeat( hand );
		}
		return rtn;
	}

	internal List<Card?> PlayNirvana( Hand hand, Card card )
	{
		List<Card?> rtn = [];
		List<Card> transfer = hand.MarketIsOpen ? hand.StashPile : hand.HasslePile;
		if( !PlayCard( hand, transfer, card ) ) { return []; }

		_ = Rules.RemoveHeat( hand ); // Cancel any heat on
		bool low = Rules.Nirvana( hand, card );
		List<string> names = [];
		foreach( Player player in Players ) // Get cards from other players
		{
			if( player.Current.Equals( hand ) ) { continue; }
			Hand other = player.Current;
			Card? steal = low ? other.LowestUnProtected : other.HighestUnProtected;
			if( steal is not null )
			{
				names.Add( $"{steal.Info.Value:#,###,##0} by {player.Name}" );
				Card.TransferCard( other.StashPile, hand.StashPile, steal );
			}
			else { names.Add( $"nothing by {player.Name}" ); }
			rtn.Add( steal );
		}
		card.AddComment( $"given {string.Join( ", ", names )} (round {hand.Round})" );
		return rtn;
	}

	internal bool PlayParanoia( Player player, Card card )
	{
		Hand hand = player.Current;
		List<Card> stash = Rules.Paranoia( hand, card );
		foreach( Card waste in stash ) // Remove cards from stash pile
		{
			waste.AddComment( $" {player.Name} played {card.Info.Caption} (round {hand.Round})" );
			Card.TransferCard( hand.StashPile, WastedPile, waste );
		}
		if( card.Name == CardInfo.cWipedOut ) // Remove cards from hassle pile (including market open)
		{
			List<Card> hassle = new( hand.HasslePile );
			foreach( Card waste in hassle )
			{
				waste.AddComment( $" {player.Name} played {card.Info.Caption} (round {hand.Round})" );
				Card.TransferCard( hand.HasslePile, WastedPile, waste );
			}
		}

		card.AddComment( $" {player.Name} played (round {hand.Round})" );
		return PlayCard( hand, WastedPile, card );
	}

#pragma warning disable CA1822 // Mark members as static
	internal bool PassCards( Dictionary<Hand, Card?> cards )
#pragma warning restore CA1822
	{
		List<Hand> hands = cards.Keys.ToList();
		int idx = 0;
		foreach( Card? pass in cards.Values )
		{
			if( pass is null ) { continue; } // Must always have something
			Hand from = hands[idx];
			if( idx == hands.Count - 1 ) { idx = 0; } else { idx++; }
			Hand to = hands[idx];
			pass.AddComment( $"passed by {from._player} (round {from.Round})" );
			PlayCard( from, to.Cards, pass );
		}

		return true;
	}

	internal static Card? GetHighPeddle( List<Card> list, bool protect = false )
	{
		List<Card> cards = protect ? Protected( list ) : Unprotected( list );
		return cards.OrderByDescending( c => c.Info.Value ).FirstOrDefault();
	}

	internal static Card? GetLowPeddle( List<Card> list, bool protect = false )
	{
		List<Card> cards = protect ? Protected( list ) : Unprotected( list );
		return cards.OrderBy( c => c.Info.Value ).FirstOrDefault();
	}

	internal Dictionary<Hand, Card?> PassOrder( Player player )
	{
		Dictionary<Hand, Card?> rtn = [];
		int idx = Players.IndexOf( player );
		for( int i = 1; i <= Players.Count; i++ )
		{
			Player p = Players[idx];
			rtn.Add( p.Current, null );
			if( Players.Count - 1 == idx ) idx = 0; else idx++;
		}
		return rtn;
	}

	internal static List<Card> Unprotected( List<Card> list )
	{
		return list.Where( c => c.Name.StartsWith( CardInfo.cPeddle ) && !c.Protected ).ToList();
	}

	#endregion

	#region Private Methods

	private static List<Card> Protected( List<Card> list )
	{
		return list.Where( c => c.Name.StartsWith( CardInfo.cPeddle ) && c.Protected ).ToList();
	}

	private bool Deal()
	{
		if( Players.Count < 2 || Players.Count > Rules.cMaxNumber ) { return false; } // Players
		for( int i = 0; i < Rules.cMaxNumber; i++ ) // Cards in hand
		{
			foreach( Player player in Players )
			{
				if( !TakeCard( player.Current ) ) { return false; };
			}
		}
		return true;
	}

	private static Player? GetBanker( List<Player> players )
	{
		foreach( Player player in players )
		{
			Card? card = CardInfo.GetFirst( player.Current.Cards, CardInfo.cBanker );
			if( card is not null ) { return player; }
		}
		return null;
	}

	#endregion

	#region Auto-Play

	private bool Auto { get; set; }

	/// <summary>Gets the banker for the current round.</summary>
	/// <returns>The player holding the banker card, or <see langword="null"/>
	/// if nobody has the card.</returns>
	[System.ComponentModel.EditorBrowsable( System.ComponentModel.EditorBrowsableState.Never )]
	public Player? GetBanker() => GetBanker( Players );

	/// <summary>Automatically plays a game.</summary>
	/// <returns><see langword="true"/> if the play completed successfully.</returns>
	/// <remarks>This method should only be used for testing purposes as it
	/// automates the decision process of which card to play.</remarks>
	[System.ComponentModel.EditorBrowsable( System.ComponentModel.EditorBrowsableState.Never )]
	public bool Play()
	{
		//if( !Auto ) { throw new InvalidOperationException( "Auto-play not enabled." ); }

		if( Auto )
		{
			while( Winner is null )
			{
				if( !StartHand() ) { return false; }
				PlayHand();
				Player? bonus = EndHand();

				foreach( Player player in Players ) // Check for winner
				{
					if( player.Total > bonus?.Total ) { bonus = player; }
				}
				if( bonus is not null && bonus.Total >= Target ) { Winner = bonus; }

				if( Winner is null ) // Reset for next hand
				{
					foreach( Player player in Players )
					{
						player.ResetCurrent();
					}
				}
			}
		}

		return true;
	}

	private bool PlayHand()
	{
		int round = 0;
		while( GrassStack.Count > 0 )
		{
			round++;
			foreach( Player player in Players )
			{
				Hand hand = player.Current;
				hand.Round = round;

				if( hand.Turns < 0 ) // Miss turns due to playing Paranoia
				{
					player.Current.Turns++;
					continue;
				}

				while( hand.Turns >= 0 )
				{
					if( !Actor.PlayerRound( this, hand ) )
					{
						if( GrassStack.Count == 0 ) { return true; } // End of grass stack
						else { return true; } // Market close played
					}

					if( player.Current.Turns > 0 ) // Extra turns due to playing Nirvana
					{
						player.Current.Turns--;
						continue;
					}
					break;
				}
			}
		}

		return false;
	}

	#endregion
}