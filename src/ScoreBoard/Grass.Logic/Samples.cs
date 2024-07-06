using Grass.Logic.Models;
namespace Grass.Logic;

/// <summary>Static Game</summary>
[System.ComponentModel.EditorBrowsable( System.ComponentModel.EditorBrowsableState.Never )]
public static class Samples
{
	private static readonly Game sGame = Game.Setup( GetPlayers() );

	/// <summary>Populate data</summary>
	public static Game Populate()
	{
		sGame.StartHand();
		// Put dealt cards back in the stack
		foreach( Player p in sGame.Players ) { BackToStack( p.Current.Cards ); }

		foreach( Player player in sGame.Players )
		{
			Hand hand = player.Current;
			hand.Round = 13;

			List<Card> to = [];
			switch( player.Name )
			{
				case "Amy":
					sGame.Take( hand, CardInfo.cOffFelony );
					sGame.Take( hand, CardInfo.cOpen );
					sGame.Take( hand, CardInfo.cOnDetained );
					sGame.Take( hand, CardInfo.cOffDetained );
					sGame.Take( hand, CardInfo.cSteal );
					sGame.Take( hand, CardInfo.cOnFelony );

					to = hand.HasslePile;
					Add( hand, CardInfo.cOnSearch, to, "by John stash was 100,000 (round 12)" );
					Add( hand, CardInfo.cOffSearch, to, "played (round 12 )" );
					Add( hand, CardInfo.cOpen, to, "trade with John (round 7)" );

					to = hand.StashPile;
					Add( hand, CardInfo.cColumbia, to, protect: true );
					Add( hand, CardInfo.cGrabaSnack, to, "played (round 9)" );
					Add( hand, CardInfo.cColumbia, to, protect: true );
					Add( hand, CardInfo.cPanama, to );
					Add( hand, CardInfo.cGrabaSnack, to, "played (round 13)" );
					break;
				case "Bob":
					sGame.Take( hand, CardInfo.cOffSearch );
					sGame.Take( hand, CardInfo.cStonehigh );
					sGame.Take( hand, CardInfo.cOnBust );
					sGame.Take( hand, CardInfo.cSteal );
					sGame.Take( hand, CardInfo.cJamaica );
					sGame.Take( hand, CardInfo.cDoublecross );

					to = hand.HasslePile;
					Add( hand, CardInfo.cOpen, to, "(round 1)" );

					to = hand.StashPile;
					Add( hand, CardInfo.cHomegrown, to );
					Add( hand, CardInfo.cPanama, to, protect: true );
					Add( hand, CardInfo.cLustConquers, to, "played (round 12)" );
					Add( hand, CardInfo.cJamaica, to );
					break;
				case "Janis":
					sGame.Take( hand, CardInfo.cClose );
					sGame.Take( hand, CardInfo.cClose );
					sGame.Take( hand, CardInfo.cBanker );
					sGame.Take( hand, CardInfo.cOffFelony );
					sGame.Take( hand, CardInfo.cLustConquers );
					sGame.Take( hand, CardInfo.cOffDetained );

					to = hand.HasslePile;
					Add( hand, CardInfo.cOpen, to, "(round 1)" );
					break;
				case "John":
					sGame.Take( hand, CardInfo.cSteal );
					sGame.Take( hand, CardInfo.cPayFine );
					sGame.Take( hand, CardInfo.cStonehigh );
					sGame.Take( hand, CardInfo.cPayFine );
					sGame.Take( hand, CardInfo.cOpen );
					sGame.Take( hand, CardInfo.cOffFelony );

					to = hand.HasslePile;
					Add( hand, CardInfo.cOnBust, to, "by Bob stash was 60,000 (round 10)" );
					Add( hand, CardInfo.cOffBust, to, "played (round 10)" );
					Add( hand, CardInfo.cOnSearch, to, "by Janis stash was 60,000 (round 11)" );
					Add( hand, CardInfo.cOffSearch, to, "played (round 11)" );
					Add( hand, CardInfo.cOpen, to, "(round 6)" );

					to = hand.StashPile;
					Add( hand, CardInfo.cMexico, to );
					Add( hand, CardInfo.cHomegrown, to );
					Add( hand, CardInfo.cPanama, to );
					Add( hand, CardInfo.cMexico, to );
					break;
			}
		}
		return sGame;
	}

	private static void BackToStack( List<Card> cards )
	{
		List<Card> temp = new( cards );
		foreach( Card card in temp ) { Card.TransferCard( cards, sGame.GrassStack, card ); }
	}

	private static void Add( Hand hand, string cardName, List<Card> to,
		string? msg = null, bool protect = false )
	{
		sGame.Take( hand, cardName );
		if( hand.Cards.Count > 6 )
		{
			Card? card = hand.Cards.LastOrDefault();
			if( card is not null )
			{
				Card.TransferCard( hand.Cards, to, card );
				if( protect ) { card.Protected = true; }
				if( msg is not null ) { card.AddComment( msg ); }
			}
		}
	}

	/// <summary>Get sample players</summary>
	public static List<Player> GetPlayers() => [
			new( "Amy" ),
			new( "Bob" ),
			new( "Janis" ),
			new( "John" ),
		];
}