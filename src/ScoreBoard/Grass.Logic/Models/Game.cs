namespace Grass.Logic.Models;

/// <summary>Provides the details and actions of a Game.</summary>
public class Game
{
	#region Constructor

	private Game( List<Player> players, int target, bool reverse, bool comment, bool auto )
	{
		Players = players;
		Target = target;
		Comment = comment;
		Auto = auto;
		Date = DateTime.Now.ToString( @"MMMM d, yyyy hh:mm tt" );
		Dealer = SetDealer( null, Hand );
		PlayOrder = [];
		ReversePlay = reverse;
	}

	#endregion

	#region Properties

	/// <summary>Number of cards left in the stack.</summary>
	public int StackCount => GrassStack.Count;

	/// <summary>List of players in the game.</summary>
	public List<Player> Players { get; private set; }

	/// <summary>Game target value.</summary>
	public int Target { get; private set; }

	/// <summary>Game date.</summary>
	public string Date { get; internal set; }

	/// <summary>Current hand number.</summary>
	public int Hand { get; internal set; }

	/// <summary>Current hand dealer.</summary>
	public Player Dealer { get; internal set; }

	/// <summary>List of players in order of play for the current hand.</summary>
	public List<Player> PlayOrder { get; private set; }

	/// <summary>Winner of the game.</summary>
	/// <returns>If the game is not completed <c>null</c> is returned.</returns>
	public Player? Winner { get; internal set; }

	/// <summary>List of cards currently in the wasted pile.</summary>
	public List<Card> WastedPile { get; private set; } = [];

	internal bool Comment { get; set; }

	internal bool ReversePlay { get; set; }

	internal Player? ParanoiaPlayer { get; set; } = null;

	internal List<Card> GrassStack { get; set; } = [];

	#endregion

	#region Public Methods

	/// <summary>Initializes a new instance of the Game class.</summary>
	/// <param name="players">List of players in the game.</param>
	/// <param name="target">Target for the game. The default is $250,000</param>
	/// <param name="reverse">Indicates whether to reverse the play order ever alternate deal.
	/// The default is <c>false</c>.</param>
	/// <param name="comment">Indicates whether to add card comments. The default is <c>true</c>.</param>
	/// <param name="auto">Indicates whether to use auto-play. The default is <c>false</c>.
	/// <br/><b>Note:</b><i> This should only be set as <c>true</c> for testing purposes
	/// as it automates the decision process of which card each player will play.</i></param>
	/// <returns>An initialized game of Grass.</returns>
	public static Game Setup( List<Player> players, int target = 250000,
		bool reverse = false, bool comment = true, bool auto = false )
	{
		Game game = new( players, target, reverse, comment, auto );
		return game;
	}

	/// <summary>Start a hand of the game.</summary>
	/// <returns><see langword="true"/> if the hand was successfully started.</returns>
	/// <remarks>There must be 104 cards and 2-6 players to play a game.</remarks>
	public bool StartHand()
	{
		GrassStack = CardInfo.BuildStack();
		if( GrassStack.Count != Rules.cCardTotal ) { return false; } // Check # of cards
		if( Players.Count < 2 || Players.Count > Rules.cMaxNumber ) { return false; } // Check # of players

		if( WastedPile.Count > 0 ) { WastedPile.Clear(); } // Reset wasted pile
		Hand++;
		return Deal();
	}

	/// <summary>End a hand of the game.</summary>
	/// <remarks>Calculates the net scores for each player, assigns the bonus to the player
	/// with the highest net score, and checks if there is a winner of the game.</remarks>
	public void EndHand()
	{
		// Calculate net scores
		Player? banker = GetBanker();
		foreach( Player player in Players )
		{
			player.Current.EndHand( banker );
			player.Completed.Add( player.Current );
		}
		banker?.Current.EndHand( null ); // Recalculate the banker net score after all skims

		// Assign the bonus to the player with the highest net score
		int high = 0;
		Player? bonus = null;
		foreach( Player player in Players )
		{
			player.Total += player.Current.NetScore;
			if( player.Current.NetScore > high )
			{
				high = player.Current.NetScore;
				bonus = player;
			}
		}
		if( bonus is not null )
		{
			bonus.Current.Bonus = Rules.cBonusAmount;
			bonus.Total += bonus.Current.Bonus;
		}

		// Check for game winner with the highest total
		high = Target;
		foreach( Player player in Players ) 
		{
			if( player.Total > high ) { high = player.Total; Winner = player; }
		}

		// If no winner reset for the next hand
		if( Winner is null )
		{
			foreach( Player player in Players ) { player.ResetCurrent(); }
			if( bonus is not null ) { Dealer = bonus; }
		}
		return;
	}

	private readonly Dictionary<Player, Card> cardsToPass = [];

	/// <summary>Add card to pass due to paranoia being played.</summary>
	/// <param name="player">Player object.</param>
	/// <param name="card">Card object.</param>
	/// <returns><see langword="false"/> if the player has already added a card or
	/// the card is not in the players hand.</returns>
	public bool AddCardToPass( Player player, Card card )
	{
		if( ParanoiaPlayer is null ) { return false; }
		if( cardsToPass.ContainsKey( player ) ) { return false; }
		if( !player.Current.Cards.Contains( card ) ) { return false; }

		cardsToPass.Add( player, card );
		if( cardsToPass.Count < Players.Count ) { return true; }

		if( cardsToPass.Count == Players.Count )
		{
			Rules.PassCards( this, cardsToPass );
			ParanoiaPlayer = null;
			cardsToPass.Clear();
		}
		return true;
	}

	/// <summary>Gets the banker for the current round.</summary>
	/// <returns>The player holding the banker card, or <see langword="null"/>
	/// if nobody has the card.</returns>
	[System.ComponentModel.EditorBrowsable( System.ComponentModel.EditorBrowsableState.Never )]
	public Player? GetBanker() => GetBanker( Players );

	#endregion

	#region Action Methods

	/// <summary>Discard a card.</summary>
	/// <param name="player">Current player.</param>
	/// <param name="card">Card to discard.</param>
	/// <returns><see langword="true"/> if the card is successfully discarded.</returns>
	public bool Discard( Player player, Card card )
	{
		Hand hand = player.Current;
		bool ok = hand.Cards.Remove( card );
		if( ok )
		{
			// TODO: Need to figure out how to trigger the pass cards function
			// This should raise an event and the Actor class needs to listen for it
			//if( card.Type == CardInfo.cParanoia )
			//{
			//	Play( player, card );
			//}
			WastedPile.Add( card );
			if( Comment ) { card.AddComment( $"{player.Name} (round {hand.Round})" ); }
		}
		return ok;
	}

	/// <summary>Play a card in the current hand.</summary>
	/// <param name="player">Current player.</param>
	/// <param name="card">Card to play.</param>
	/// <returns>A <see cref="Grass.Logic.PlayResult" /> object representing the results
	/// of the play.</returns>
	/// <remarks>The <c>null</c> return value is used to indicate success. The result should be compared
	/// to <see cref="Grass.Logic.PlayResult.Success"/> rather than checking for <c>null</c>.</remarks>
	public PlayResult Play( Player player, Card card ) =>
		Rules.Play( this, player, card );

	/// <summary>Play a card in the current hand with another player.</summary>
	/// <param name="player">Current player.</param>
	/// <param name="card">Card to play.</param>
	/// <param name="with">Other player.</param>
	/// <param name="other">Other players card.</param>
	/// <returns>A <see cref="Grass.Logic.PlayResult" /> object representing the results
	/// of the play.</returns>
	/// <remarks>The <c>null</c> return value is used to indicate success. The result should be compared
	/// to <see cref="Grass.Logic.PlayResult.Success"/> rather than checking for <c>null</c>.</remarks>
	public PlayResult Play( Player player, Card card, Player with, Card other ) =>
		Rules.Play( this, player, card, with, other );

	/// <summary>Protect peddle cards.</summary>
	/// <param name="player">Current player.</param>
	/// <param name="card">Card to play.</param>
	/// <param name="peddles">List of peddle cards to protect</param>
	/// <returns>A <see cref="Grass.Logic.PlayResult" /> object representing the results
	/// of the play.</returns>
	/// <remarks>The <c>null</c> return value is used to indicate success. The result should be compared
	/// to <see cref="Grass.Logic.PlayResult.Success"/> rather than checking for <c>null</c>.</remarks>
	public PlayResult Protect( Player player, Card card, List<Card> peddles ) =>
		Rules.Protect( this, player, card, peddles );

	/// <summary>Take the next card from the stack.</summary>
	/// <param name="hand">Current player hand to add the card to.</param>
	/// <returns><see langword="true"/> if the card is successfully taken
	/// from the stack and added to the players hand.</returns>
	public bool Take( Hand hand )
	{
		if( GrassStack.Count == 0 ) { return false; }
		Card card = GrassStack[^1];
		bool rtn = GrassStack.Remove( card );
		if( rtn ) { hand.Cards.Add( card ); }
		return rtn;
	}

	internal bool Take( Hand hand, string cardName )
	{
		if( GrassStack.Count == 0 ) { return false; }
		Card? card = GrassStack.Where( c => c.Id == cardName ).FirstOrDefault();
		if( card == null ) { return false; }
		bool rtn = GrassStack.Remove( card );
		if( rtn ) { hand.Cards.Add( card ); }
		return rtn;
	}

	#endregion

	#region Private Methods

	private Player SetDealer( Player? dealer, int handNumber )
	{
		if( dealer is null )
		{
			// Pick a random player if players populated
			Random random = new();
			int idx = random.Next( 0, Players.Count );
			dealer = Players[idx];
		}
		if( handNumber == 0 ) { return dealer; }

		// Order players for hand
		if( PlayOrder.Count > 0 ) { PlayOrder.Clear(); }
		List<Player> order = new( Players );
		bool reverse = handNumber % 2 == 0;
		if( ReversePlay & reverse ) { order.Reverse(); } // every alternate hand

		int start = order.FindIndex( x => x == dealer ) + 1;
		for( int i = start; PlayOrder.Count < Players.Count; i++ )
		{
			if( i == Players.Count ) { i = 0; }
			PlayOrder.Add( order[i] );
		}
		return dealer;
	}

	internal bool Deal()
	{
		Dealer = SetDealer( Dealer, Hand );
		for( int i = 0; i < Rules.cMaxNumber; i++ ) // Cards in hand
		{
			foreach( Player player in PlayOrder )
			{
				if( !Take( player.Current ) ) { return false; };
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

	internal bool Auto { get; set; }

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
				EndHand();
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
			foreach( Player player in PlayOrder )
			{
				Hand hand = player.Current;
				hand.Round = round;

				// Miss turns due to playing Paranoia
				if( hand.Turns < 0 )
				{
					player.Current.Turns++;
					continue;
				}

				while( hand.Turns >= 0 )
				{
					if( !Actor.PlayRound( this, hand ) )
					{
						if( GrassStack.Count == 0 ) { return true; } // End of grass stack
						else { return true; } // Market close played
					}

					// Extra turns due to playing Nirvana
					if( player.Current.Turns > 0 )
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