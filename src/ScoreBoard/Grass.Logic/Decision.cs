// Ignore Spelling: Feelgood, rtn
using Grass.Logic.Models;
namespace Grass.Logic;

internal class Decision
{
	private static int TotalHighUnprotected = 0;
	private static int TotalLowUnprotected = 0;
	internal static Decision? FeelgoodInPlay = null;

	#region Properties and Constructor

	internal Player Player { get; private set; }
	internal Hand Hand { get; private set; }
	internal int TotalTabled => Hand.Protected + Hand.UnProtected;
	internal bool NeverOpen { get; private set; }
	internal bool IsHeatOn { get; private set; }

	private Card? LowUnprotected => Hand.LowestUnProtected;
	private int LowValue => LowUnprotected is not null ? LowUnprotected.Info.Value : 0;
	private Card? HighUnprotected => Hand.HighestUnProtected;
	private int HighValue => HighUnprotected is not null ? HighUnprotected.Info.Value : 0;
	private bool DrFeelgood { get; set; }

	internal Decision( Player player )
	{
		Player = player;
		Hand = player.Current;
		NeverOpen = !CardInfo.GetCards( Hand.HasslePile, CardInfo.cOpen ).Any();
		IsHeatOn = !NeverOpen && Hand.HasslePile.Count > 1;
		Card? card = Hand.StashPile.FirstOrDefault( c => c.Name == CardInfo.cDrFeelgood );
		if( card is not null ) { DrFeelgood = true; }
	}

	/// <inheritdoc/>
	public override string ToString() => Player.Name;

	#endregion

	internal static void SetTotals( List<Decision> others )
	{
		TotalHighUnprotected = 0;
		TotalLowUnprotected = 0;
		FeelgoodInPlay = null;
		foreach( Decision d in others )
		{
			TotalLowUnprotected += d.LowValue;
			TotalHighUnprotected += d.HighValue;
			if( d.DrFeelgood && FeelgoodInPlay == null ) { FeelgoodInPlay = d; }
		}
	}

	internal static Decision? Trade( Decision data, List<Decision> others, string required )
	{
		// Must have money in hand
		Card? low = Game.GetLowPeddle( data.Hand.Cards );
		if( low is null ) { return null; } // No money to trade
		Decision? other = null;

		// Another player must have the card to trade
		foreach( Decision trade in others )
		{
			List<Card> res = CardInfo.GetCards( trade.Hand.Cards, required ).ToList();
			if( res.Count == 0 ) { continue; }
			switch( required )
			{
				case CardInfo.cOpen:
					// Must be open and have 1 in hand or not be open and have more than 1 in hand
					int num = trade.NeverOpen ? 1 : 0;
					if( res.Count > num ) { other = trade; }
					break;
				default:
					if( res.Count > 0 ) { other = trade; }
					break;
			}
			if( other is not null ) { break; } // Found other player
		}
		return other;
	}

	internal static bool Close( Decision data, Game game )
	{
		// Must have score greater than target
		int score = data.TotalTabled + data.Player.Total + data.Hand.CurrentNet();
		int target = game.Target; // 200000; game.Target

		if( score > target ) { return true; }
		return false;
	}

	internal static Card? HeatOff( Decision data, Card? heatOn )
	{
		if( heatOn is null ) { return null; }
		List<Card> list = CardInfo.GetCards( data.Hand.Cards, CardInfo.cHeatOff ).ToList();
		if( list.Count == 0 ) { return null; } // No cards in hand

		// Match heat off card with heat on
		Card? rtn = heatOn.Name switch
		{
			CardInfo.cOnBust => CardInfo.GetFirst( list, CardInfo.cOffBust ),
			CardInfo.cOnDetained => CardInfo.GetFirst( list, CardInfo.cOffDetained ),
			CardInfo.cOnFelony => CardInfo.GetFirst( list, CardInfo.cOffFelony ),
			CardInfo.cOnSearch => CardInfo.GetFirst( list, CardInfo.cOffSearch ),
			_ => null
		} ?? CardInfo.GetFirst( list, CardInfo.cPayFine ); // Check for pay fine
		if( rtn is null ) { return null; }

		// cPayFine - Must have small unprotected card in stash
		//            worth using if large unprotected and high value peddle in hand
		if( rtn.Name == CardInfo.cPayFine )
		{
			Card? fine = data.LowUnprotected;
			if( fine is null || fine.Info.Value > 25000 ) { return null; }
		}

		return rtn;
	}

	internal static Card? Protect( Decision data, List<Card> list )
	{
		// Must have unprotected card in stash with same value
		// cCatchabuzz      single $25,000 or multiple lower values
		// cGrabasnack      single $25,000 or multiple lower values
		// cLustconquersall single $50,000 or multiple lower values
		List<Card> stash = Game.Unprotected( data.Hand.StashPile );
		if( stash.Count == 0 ) { return null; }

		// Match unprotected stash card using highest protection value first
		Card? rtn = null;
		foreach( Card protect in list.OrderByDescending( x => x.Info.Value ).ToList() )
		{
			foreach( Card peddle in stash )
			{
				if( peddle.Info.Value != protect.Info.Value ) { continue; }
				peddle.Protected = true;
				protect.AddComment( $"played (round {data.Hand.Round})" ); // TODO: This s/b in Game
				rtn = protect;
				break;
			}
		}
		return rtn;
	}

	internal static Card? Peddle( Decision data )
	{
		// Don't play Dr. Feelgood if no other unprotected cards in stack
		Card? rtn = data.Hand.UnProtected == 0
			? Game.GetLowPeddle( data.Hand.Cards )
			: Game.GetHighPeddle( data.Hand.Cards );

		return rtn;
	}

	internal static Dictionary<Hand, Card?> Steal( Decision data )
	{
		Dictionary<Hand, Card?> rtn = [];

		// Don't steal Dr. Feelgood if no other unprotected cards in stack

		// Currently only works for Dr. Feelgood 
		if( FeelgoodInPlay is null ) { return rtn; }
		Decision other = FeelgoodInPlay;
		Card? steal = CardInfo.GetFirst( other.Hand.StashPile, CardInfo.cDrFeelgood );

		// My official rules state other CAN have heat on
		if( other.IsHeatOn ) { return rtn; }
		rtn.Add( other.Hand, steal );

		return rtn;
	}

	internal static Card? Nirvana( Decision data, List<Card> list )
	{
		// cStonehigh should play if any player just has cDrFeelgood as only unprotected stash?
		//            should play if others have large amount of unprotected
		// cEuphoria  shouldn't play if very little other's unprotected
		//            should play if market not open and very little unprotected?
		//            shouldn't play if Dr. Feelgood not on table?

		Card? rtn = null;
		foreach( Card pick in list ) // Pick a card from the list
		{
			if( pick.Name == CardInfo.cEuphoria && TotalHighUnprotected > 105000 ) { rtn = pick; }
			else if( TotalLowUnprotected > 50000 ) { rtn = pick; }
			if( rtn is not null ) { break; }
		}
		return rtn;
	}

	internal static Decision? HeatOn( Decision data, List<Decision> others )
	{
		// Find a player without heat on and a score greater than 55,000 
		int val = 55000;
		Decision? rtn = null;
		foreach( Decision other in others )
		{
			if( !other.Hand.MarketIsOpen ) { continue; }
			if( other.TotalTabled > val )
			{
				val = other.TotalTabled;
				rtn = other;
				continue;
			}
		}

		return rtn;
	}

	internal static Card? Paranoia( Decision data, List<Card> list )
	{
		// cSoldout     lowest tabled unprotected peddle should be 25,000 or less
		//              Utterly wiped out is worst card
		// cDoublecross Highest tabled unprotected peddle should be 50,000 or less
		//              Utterly wiped out is worst card
		// cWipedOut    Shouldn't play unless market open is available or Dr Feelgood is in stack
		//              Shouldn't play if large amount of unprotected

		Card? rtn = null;
		Card? pass = null;
		// Process in order of severity, cSoldout, cDoublecross, cWipedOut
		// TODO: Should this be the other way round?
		foreach( Card card in list.OrderByDescending( x => x.Info.Value ).ToList() )
		{
			pass = GetWorstCard( data.Hand.Cards, card );
			bool wipeOut = ( pass is not null ) && ( pass.Name == CardInfo.cWipedOut );

			switch( card.Name )
			{
				case CardInfo.cSoldout:
					if( data.LowValue <= 25000 || wipeOut )
					{
						// verified with lowest 5,000 and with 2 protected and no other
						if( data.LowUnprotected is not null )
						{
							rtn = card;
						}
					}
					break;
				case CardInfo.cDoublecross:
					if( data.HighValue <= 50000 || wipeOut )
					{
						if( data.HighUnprotected is not null )
						{
							rtn = card;
						}
					}
					break;
				case CardInfo.cWipedOut:
					if( data.NeverOpen || data.TotalTabled == 0 )
					{
						rtn = card; // Manually set to debug removal of hassle pile
					}
					break;
			}
			if( rtn is not null ) { break; }
		}

		return rtn;
	}

	#region Card to discard

	internal static bool Discard( Hand hand, Game game )
	{
		List<Card> pile = game.WastedPile;
		Card? rtn = GetDiscard( hand.Cards );

#pragma warning disable IDE0074 // Use compound assignment
#pragma warning disable IDE0270 // Null check can be simplified
		if( rtn is null ) { rtn = hand.Cards[0]; } // Cannot be null
#pragma warning restore IDE0270 // Null check can be simplified
#pragma warning restore IDE0074 // Use compound assignment

		bool success = Game.PlayCard( hand, pile, rtn );
		if( success ) { rtn.AddComment( $" {hand._player} (round {hand.Round})" ); }
		return success;
	}

	private static Card? GetDiscard( List<Card> cards )
	{
		Card? rtn = CardInfo.GetFirst( cards, CardInfo.cOnBust );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cOnDetained );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cOnFelony );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cOnSearch );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cOffBust );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cOffDetained );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cOffFelony );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cOffSearch );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cPayFine );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cOpen );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cClose );

		rtn ??= CardInfo.GetFirst( cards, CardInfo.cHomegrown );    // 5,000
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cMexico );       // 5,000
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cColumbia );     // 25,000
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cJamaica );      // 25,000
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cSteal );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cCatchaBuzz );   // 25,000
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cGrabaSnack );   // 25,000
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cLustConquers ); // 50,000
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cPanama );       // 50,000

		// TODO: Need to play these - they cannot be discarded
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cSoldout );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cDoublecross );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cWipedOut );

		rtn ??= CardInfo.GetFirst( cards, CardInfo.cStonehigh );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cEuphoria );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cDrFeelgood );   // 100,000
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cBanker );

		return rtn;
	}

	#endregion

	#region Card to pass

	internal static Card GetWorstCard( List<Card> cards, Card? ignore = null )
	{
		Card? rtn = GetCard( cards, CardInfo.cWipedOut, ignore );
		rtn ??= GetCard( cards, CardInfo.cDoublecross, ignore );
		rtn ??= GetCard( cards, CardInfo.cSoldout, ignore );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cOffBust );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cOffDetained );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cOffFelony );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cOffSearch );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cPayFine );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cClose );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cOpen );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cHomegrown );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cMexico );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cColumbia );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cJamaica );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cOnBust );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cOnDetained );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cOnFelony );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cOnSearch );

		rtn ??= CardInfo.GetFirst( cards, CardInfo.cCatchaBuzz );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cGrabaSnack );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cLustConquers );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cStonehigh );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cSteal );
		rtn ??= CardInfo.GetFirst( cards, CardInfo.cPanama );
		// rtn ??= CardInfo.GetFirst( cards, CardInfo.cDrFeelgood );
		//rtn ??= CardInfo.GetFirst( cards, CardInfo.cEuphoria );
		//rtn ??= CardInfo.GetFirst( cards, CardInfo.cBanker );

#pragma warning disable IDE0074 // Use compound assignment
		if( rtn is null ) { rtn = cards[0]; } // Cannot be null
#pragma warning restore IDE0074 // Use compound assignment

		return rtn;
	}

	private static Card? GetCard( List<Card> cards, string name, Card? ignore )
	{
		Card? rtn = CardInfo.GetFirst( cards, name );
		if( rtn is not null && ignore is not null && rtn.Equals( ignore ) ) { rtn = null; }
		return rtn;
	}

	#endregion
}