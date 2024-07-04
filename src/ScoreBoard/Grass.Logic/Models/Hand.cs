namespace Grass.Logic.Models;

/// <summary>Provides the details of a players hand.</summary>
public class Hand
{
	/// <summary>Hand number within a game.</summary>
	public int Number { get; internal set; }

	/// <summary>Determines whether the market is open.</summary>
	public bool MarketIsOpen => Rules.IsMarketOpen( HasslePile );

	/// <summary>List of all cards currently in the hassle pile.</summary>
	public List<Card> HasslePile { get; private set; } = [];

	/// <summary>List of all cards currently in the stash pile.</summary>
	public List<Card> StashPile { get; private set; } = [];

	/// <summary>List of peddle and protection cards currently in the stash pile.</summary>
	public List<Card> StashView { get { return GetStashView(); } }

	/// <summary>Highest peddle card value held at the end of the hand.</summary>
	public int HighestPeddle { get { return GetHighValue( Cards ); } }

	/// <summary>Total of unprotected cards in the stash at the end of the hand.</summary>
	public int UnProtected { get { return TotalStash( StashPile, false ); } }

	/// <summary>Total of protected cards in the stash at the end of the hand.</summary>
	public int Protected { get { return TotalStash( StashPile, true ); } }

	/// <summary>Amount skimmed by the Banker at the end of the hand.</summary>
	public int Skimmed { get; internal set; }

	/// <summary>Total of all paranoia cards held at the end of the hand.</summary>
	public int ParanoiaFines { get; private set; }

	/// <summary>Net score at the end of the hand.</summary>
	/// <remarks>This is the total peddle money in the stash minus all paranoia
	/// card fines and the highest peddle card value held.</remarks>
	public int NetScore { get; private set; }

	/// <summary>Bonus amount at the end of the hand.</summary>
	public int Bonus { get; internal set; }

	/// <inheritdoc/>
	[System.ComponentModel.EditorBrowsable( System.ComponentModel.EditorBrowsableState.Never )]
	public override string ToString() => Player;

	internal Hand() { }

	internal int Turns { get; set; }

	internal string Player { get; set; } = "Unknown";

	internal int Round { get; set; }

	internal Card? HighestUnProtected { get { return Game.GetHighPeddle( StashPile ); } }

	internal Card? LowestUnProtected { get { return Game.GetLowPeddle( StashPile ); } }

	internal List<Card> Cards { get; private set; } = [];

	internal void EndHand( Player? banker )
	{
		int work = 0;
		foreach( Card card in CardInfo.Paranoia( Cards ) ) { work += card.Info.Value; }
		ParanoiaFines = work;

		if( banker is not null )
		{
			int skim = Rules.SkimAmt( this, banker );
			Skimmed -= skim;
			banker.Current.Skimmed += skim;
		}
		NetScore = Protected + UnProtected + ParanoiaFines + Skimmed - HighestPeddle;
	}

	internal int CurrentNet()
	{
		int rtn = 0;
		foreach( Card card in CardInfo.Paranoia( Cards ) ) { rtn += card.Info.Value; }
		rtn -= HighestPeddle;
		// TODO: Included current skimmed amount
		return rtn;
	}

	#region Private Methods

	private static int GetHighValue( List<Card> cards )
	{
		Card? card = Game.GetHighPeddle( cards );
		return card is not null ? card.Info.Value : 0;
	}

	private static int TotalStash( List<Card> list, bool protect )
	{
		int rtn = 0;
		IEnumerable<Card> peddle = CardInfo.GetCards( list, CardInfo.cPeddle );
		foreach( Card card in peddle ) { if( card.Protected == protect ) { rtn += card.Info.Value; } }
		return rtn;
	}

	private List<Card> GetStashView()
	{
		return StashPile.Where( c => c.Name.StartsWith( CardInfo.cProtection )
		   || ( c.Name.StartsWith( CardInfo.cPeddle ) && !c.Protected ) )
		 .OrderByDescending( c => c.Info.Value ).ThenBy( c => c.Info.Name ).ToList();
	}

	#endregion
}