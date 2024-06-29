// Ignore Spelling: Feelgood
using Grass.Logic.Models;
namespace Grass.Logic;

/// <summary>Auto-play Actor.</summary>
internal class Actor
{
	protected static Game sGame = new( new List<Player>() );

	internal static bool PlayerRound( Game game, Hand current )
	{
		#region Populate decision data

		if( !sGame.Equals( game ) ) { sGame = game; }
		Decision? data = null;
		List<Decision> others = [];
		foreach( Player p in game.Players )
		{
			Decision temp = new( p );
			if( p.Current == current ) { data = temp; }
			else { others.Add( temp ); }
		}
		if( data is null ) { return false; }

		Decision.SetTotals( others );

		#endregion

		// If picked up Utterly wiped out try and use paranoia to get rid of it

		if( !game.TakeCard( data.Hand ) ) { return false; }         // No cards left in stack

		if( Open( data, others ) is not null ) { return true; };    // Market needs to be open
		if( HeatOff( data, others ) is not null ) { return true; }; // Heat needs to be removed
		if( Close( data ) is not null ) { return false; };          // Game over
		if( Protect( data ) is not null ) { return true; };         // Protection
		if( Peddle( data ) is not null ) { return true; };          // Peddle
		if( Steal( data ) is not null ) { return true; };           // Skim
		if( Nirvana( data ) is not null ) { return true; };         // Nirvana
		if( HeatOn( data, others ) is not null ) { return true; };  // Heat on
		if( Paranoia( data ) is not null ) { return true; };        // Paranoia

		bool res = Decision.Discard( data.Hand, sGame );            // Discard
		if( data.Hand.Cards.Count != Rules.cMaxNumber ) { res = false; }
		foreach( Decision d in others ) { if( d.Hand.Cards.Count != Rules.cMaxNumber ) { res = false; } }
		return res;
	}

	private static Card? Open( Decision data, List<Decision> others )
	{
		Card? rtn = CardInfo.GetFirst( data.Hand.Cards, CardInfo.cOpen );
		if( rtn is null && data.NeverOpen ) { rtn = Trade( data, others, CardInfo.cOpen ); }
		if( rtn is not null && data.NeverOpen )
		{
			bool ok = Game.PlayCard( data.Hand, data.Hand.HasslePile, rtn );
			if( ok && rtn.Caption.Length == 0 ) { rtn.AddComment( $"played (round {data.Hand.Round})" ); }
			return ok ? rtn : null;
		}
		return null;
	}

	private static Card? Trade( Decision data, List<Decision> others, string cardName )
	{
		if( string.IsNullOrWhiteSpace( cardName ) ) { return null; }
		Decision? other = Decision.Trade( data, others, cardName );
		if( other is null ) { return null; } // No other player

		Card? low = Game.GetLowPeddle( data.Hand.Cards );
		if( low is null ) { return null; } // No low unprotected
		Card? card = CardInfo.GetFirst( other.Hand.Cards, cardName );
		if( card is null ) { return null; }

		bool ok = Game.TradeCard( data.Hand, low, other.Hand, card );
		if( ok ) { card.AddComment( $"trade with {other.Player.Name} (round {data.Hand.Round})" ); }
		return ok ? card : null;
	}

	private static Card? HeatOff( Decision data, List<Decision> others )
	{
		if( !data.IsHeatOn ) { return null; }

		Card? heat = CardInfo.GetLast( data.Hand.HasslePile, CardInfo.cHeatOn );
		if( heat is null ) { return null; }
		List<Card> list = CardInfo.GetCards( data.Hand.Cards, CardInfo.cHeatOff ).ToList();
		if( list.Count == 0 ) // No cards in hand
		{
			// Try and make trade
			string name = heat.Name.Replace( CardInfo.cHeatOn, CardInfo.cHeatOff );
			Card? trade = Trade( data, others, name );
			if( trade is null ) { return null; }
		}

		Card? card = Decision.HeatOff( data, heat );
		if( card is null ) { return null; }

		Card? fine = null;
		if( card.Name == CardInfo.cPayFine ) { fine = Game.GetLowPeddle( data.Hand.StashPile ); }
		bool ok = sGame.PlayHeatOff( data.Hand, card, fine );
		if( ok )
		{
			card.AddComment( $"played (round {data.Hand.Round})" );
			fine?.AddComment( $" {data.Player.Name} paid fine (round {data.Hand.Round})" );
		}
		return ok ? card : null;
	}

	private static Card? Close( Decision data )
	{
		if( !Rules.CanPlayCard( data.Hand, CardInfo.cClose ) ) { return null; }
		Card? rtn = CardInfo.GetFirst( data.Hand.Cards, CardInfo.cClose );
		if( rtn is null ) { return null; }

		if( !Decision.Close( data, sGame ) ) { return null; }

		bool ok = Game.PlayCard( data.Hand, data.Hand.HasslePile, rtn );
		if( ok ) { rtn.AddComment( $"played (round {data.Hand.Round})" ); }
		return ok ? rtn : null;
	}

	private static Card? Protect( Decision data )
	{
		if( !Rules.CanPlayCard( data.Hand, CardInfo.cProtection ) ) { return null; }
		List<Card> list = CardInfo.GetCards( data.Hand.Cards, CardInfo.cProtection ).ToList();
		if( list.Count == 0 ) { return null; } // No card in hand

		Card? rtn = Decision.Protect( data, list );
		if( rtn is null ) { return null; }

		bool ok = Game.PlayCard( data.Hand, data.Hand.StashPile, rtn );
		return ok ? rtn : null;
	}

	private static Card? Peddle( Decision data )
	{
		if( !Rules.CanPlayCard( data.Hand, CardInfo.cPeddle ) ) { return null; }

		Card? rtn = Decision.Peddle( data );
		if( rtn is null ) { return null; }

		bool ok = Game.PlayCard( data.Hand, data.Hand.StashPile, rtn );
		return ok ? rtn : null;
	}

	private static Card? Steal( Decision data )
	{
		if( !Rules.CanPlayCard( data.Hand, CardInfo.cSteal ) ) { return null; }
		Card? rtn = CardInfo.GetFirst( data.Hand.Cards, CardInfo.cSteal );
		if( rtn is null ) { return null; } // No card in hand

		Dictionary<Hand, Card?> steal = Decision.Steal( data );
		if( steal.Count == 0 ) { return null; }

		KeyValuePair<Hand, Card?> test = steal.First();
		Hand hand = test.Key;
		Card? card = test.Value;
		if( card is null ) { return null; }

		bool ok = Rules.SkimCard( data.Hand, rtn, hand, card );
		if( ok )
		{
			rtn.AddComment( data.Player.Name + $" stole {card.Info.Caption} (round {data.Hand.Round})" );
			if( card.Name == CardInfo.cDrFeelgood ) { Decision.FeelgoodInPlay = data; }
		}
		return ok ? rtn : null;
	}

	private static Card? Nirvana( Decision data )
	{
		if( !Rules.CanPlayCard( data.Hand, CardInfo.cNirvana ) ) { return null; }
		List<Card> list = CardInfo.GetCards( data.Hand.Cards, CardInfo.cNirvana ).ToList();
		if( list.Count == 0 ) { return null; } // No card in hand

		Card? rtn = Decision.Nirvana( data, list );
		if( rtn is null ) { return null; }

		List<Card?> stolen = sGame.PlayNirvana( data.Hand, rtn );
		bool success = stolen.Count > 0;
		foreach( Card? card in stolen )
		{
			if( card?.Name == CardInfo.cDrFeelgood ) { Decision.FeelgoodInPlay = data; }
		}
		return success ? rtn : null;
	}

	private static Card? HeatOn( Decision data, List<Decision> others )
	{
		if( !Rules.CanPlayCard( data.Hand, CardInfo.cHeatOn ) ) { return null; }
		List<Card> list = CardInfo.GetCards( data.Hand.Cards, CardInfo.cHeatOn ).ToList();
		if( list.Count == 0 ) { return null; } // No card in hand

		Decision? other = Decision.HeatOn( data, others );
		if( other is null ) { return null; }

		Card rtn = list[0];
		bool ok = Game.PlayCard( data.Hand, other.Hand.HasslePile, rtn );
		if( ok ) { rtn.AddComment( $"by {data.Player.Name} when stash was {other.TotalTabled:###,##0} (round {data.Hand.Round})" ); }
		return ok ? rtn : null;
	}

	private static Card? Paranoia( Decision data )
	{
		if( !Rules.CanPlayCard( data.Hand, CardInfo.cParanoia ) ) { return null; }
		List<Card> list = CardInfo.Paranoia( data.Hand.Cards ).ToList();
		if( list.Count == 0 ) { return null; } // No card in hand

		Card? rtn = Decision.Paranoia( data, list );
		if( rtn is null ) { return null; }

		// Determine the card for each player to pass
		Card? pass = Decision.GetWorstCard( data.Hand.Cards, rtn ); // pass worst!
		Dictionary<Hand, Card?> order = sGame.PassOrder( data.Player );
		foreach( Hand h in order.Keys )
		{
			order[h] = h.Equals( data.Hand ) ? pass : Decision.GetWorstCard( h.Cards );
		}

		bool ok = sGame.PlayParanoia( data.Player, rtn );
		_ = sGame.PassCards( order );
		return ok ? rtn : null;
	}

	internal static List<CardInfo> ImageInfo() => CardInfo.Info();
}