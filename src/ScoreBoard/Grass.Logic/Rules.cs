using Grass.Logic.Models;
namespace Grass.Logic;

internal class Rules
{
	internal const int cCardTotal = 104;
	internal const int cMaxNumber = 6; // Maximum players and cards in hand
	internal const int cBonusAmount = 25000;

	#region Internal Methods

	internal static bool IsMarketOpen( List<Card> list )
	{
		Card? card = list.LastOrDefault();
		if( card == null || card.Name != CardInfo.cOpen ) { return false; }
		return true;
	}

	internal static bool RemoveHeat( Hand hand )
	{
		Card? card = CardInfo.GetFirst( hand.HasslePile, CardInfo.cOpen );
		if( card is null || hand.MarketIsOpen ) { return false; }

		bool rtn = hand.HasslePile.Remove( card );
		if( rtn ) { hand.HasslePile.Add( card ); }
		return rtn;
	}

	internal static int SkimAmt( Hand hand, Player banker )
	{
		if( hand == banker.Current ) { return 0; }
		int rtn = hand.UnProtected / 100 * 20; // Banker skims 20% of unprotected in stash pile
		return rtn;
	}

	internal static bool CanPlayCard( Hand hand, string name )
	{
		switch( name )
		{
			case CardInfo.cPeddle:
			case CardInfo.cProtection:
			case CardInfo.cSteal:
			case CardInfo.cClose:
				if( !hand.MarketIsOpen ) { return false; } // Cannot play if heat on
				break;
			case CardInfo.cHeatOff:
				if( hand.MarketIsOpen ) { return false; } // Cannot play if no heat on
				break;
			case CardInfo.cNirvana:
				return CardInfo.GetCards( hand.HasslePile, CardInfo.cOpen ).Any(); // Market must have been opened
			case CardInfo.cHeatOn:
				break;
			case CardInfo.cParanoia:
				break;
		}

		return true;
	}

	internal static bool Nirvana( Hand hand, Card card )
	{
		bool rtn = true;
		switch( card.Name )
		{
			case CardInfo.cStonehigh:
				hand.Turns += 1; // 1 extra turn
				rtn = true; // Lowest tabled unprotected peddle for all others
				break;
			case CardInfo.cEuphoria:
				hand.Turns += 2; // 2 extra turns
				rtn = false; // Highest tabled unprotected peddle for all others
				break;
		}
		return rtn;
	}

	internal static List<Card> Paranoia( Hand hand, Card card )
	{
		List<Card> rtn = [];
		switch( card.Name )
		{
			case CardInfo.cSoldout:
				hand.Turns -= 1; // Miss 1 turn and lose lowest unprotected peddle
				if( hand.LowestUnProtected is not null ) { rtn.Add( hand.LowestUnProtected ); }
				break;
			case CardInfo.cDoublecross:
				hand.Turns -= 2; // Miss 2 turns and lose highest unprotected peddle
				if( hand.HighestUnProtected is not null ) { rtn.Add( hand.HighestUnProtected ); }
				break;
			case CardInfo.cWipedOut:
				hand.Turns -= 2; // Miss 2 turns
				rtn.AddRange( hand.StashPile ); // Lose everything in stash pile
				break;
		}
		return rtn;
	}
	#endregion

	// TODO: Could be in Game class!!

	internal static bool SkimCard( Hand yourHand, Card card, Hand otherHand, Card otherCard )
	{
		if( card.Name != CardInfo.cSteal ) { return false; }

		// Both hands must be hassle free
		//if( !yourHand.MarketIsOpen || !otherHand.MarketIsOpen ) { return false; }

		bool rtn = otherHand.StashPile.Remove( otherCard );
		if( rtn )
		{
			yourHand.Cards.Remove( card );
			otherHand.StashPile.Add( card );
			yourHand.StashPile.Add( otherCard );
		}

		return rtn;
	}
}