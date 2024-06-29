using Grass.Logic.Models;
using System.Diagnostics;
namespace Grass.Auto;

internal class Program
{
	private static readonly Stopwatch sStopWatch = new();

	static void Main()
	{
		try
		{
			sStopWatch.Start();
			Game game = Game.Start( GetPlayers(), auto: true );
			if( !game.Play() ) { Console.WriteLine( "Failed to successfully auto-play game." ); }
			else
			{
				sStopWatch.Stop();
				HtmlBuilder.CreateHtml( game );
				//ShowResults( game );
			}
		}
		catch( Exception ex ) { Console.WriteLine( ex.ToString() ); }
		finally
		{
			Console.WriteLine();
			Console.WriteLine( $"Runtime: {sStopWatch.Elapsed.TotalMilliseconds} milliseconds" );
			if( Environment.UserInteractive )
			{
				Console.Write( @"Press any key to continue . . . " );
				_ = Console.Read();
			}
		}
	}

	private static List<Player> GetPlayers() => [
			new( "Amy" ),
			new( "Bob" ),
			new( "Janis" ),
			new( "John" ),
		];

	internal static void ShowResults( Game game )
	{
		Console.WriteLine( $"Game target: {game.Target:$#,###,##0} on {game.Date}" );
		Player? winner = game.Winner;
		Player? banker = game.GetBanker();
		if( banker is not null ) { Console.WriteLine( $"{banker.Name} is the Banker" ); }
		if( winner is not null )
		{
			string str = winner.Current.NetScore >= game.Target ? "game" : "hand";
			Console.WriteLine( $"{winner.Name} won {str} with {winner.Total:$#,###,##0}" );
		}
		string reason = game.StackCount == 0 ? "Stack ran out." : "Market closed.";
		Console.WriteLine( $"{game.Players[0].GetLastRoundCount()} rounds played - {reason}" );
		Console.WriteLine();

		Console.WriteLine( $"Wasted pile count: {game.WastedPile.Count}" );
		foreach( Card card in game.WastedPile ) { Console.WriteLine( card + card.Caption ); }

		ConsoleColor dftColor = Console.ForegroundColor;
		foreach( Player player in game.Players )
		{
			Hand? hand = player.GetLastRound();
			if( hand is null ) { continue; }
			Console.WriteLine();
			Console.WriteLine( "------------------------------" );
			Console.WriteLine( $"{player}" );

			Console.WriteLine( "-- hassle:" );
			Console.ForegroundColor = ConsoleColor.Red;
			foreach( Card card in hand.HasslePile ) { Console.WriteLine( $"{card} {card.Caption}" ); }
			Console.ForegroundColor = dftColor;

			Console.WriteLine( "-- in hand:" );
			Console.ForegroundColor = ConsoleColor.Yellow;
			foreach( Card card in player.GetLastRoundCards() ) { Console.WriteLine( $"{card} {card.Caption}" ); }
			Console.ForegroundColor = dftColor;

			Console.WriteLine( "-- stash:" );
			Console.ForegroundColor = ConsoleColor.Green;
			foreach( Card card in hand.StashPile ) { Console.WriteLine( $"{card} {card.Caption}" ); }
			Console.ForegroundColor = dftColor;

			Console.WriteLine( "-- score:" );
			Console.WriteLine( $"Protected......: {hand.Protected:###,##0}" );
			Console.WriteLine( $"UnProtected....: {hand.UnProtected:###,##0}" );
			Console.WriteLine( $"Skimmed........: {hand.Skimmed:###,##0}" );
			if( hand.HighestPeddle > 0 ) Console.WriteLine( $"Highest Peddle.: {hand.HighestPeddle:-###,##0}" );
			else Console.WriteLine( $"Highest Peddle.: {hand.HighestPeddle:###,##0}" );
			Console.WriteLine( $"Paranoia Fines.: {hand.ParanoiaFines:###,##0}" );
			Console.WriteLine( $"Net Score......: {hand.NetScore:###,##0}" );
			if( hand.Bonus > 0 ) { Console.WriteLine( $"Win hand Bonus: {hand.Bonus:###,##0}" ); }

			//HtmlBuilder.CreateHtml( game );
		}
	}
}