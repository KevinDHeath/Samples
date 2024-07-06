using Grass.Logic.Models;
using System.Diagnostics;
namespace Grass.Auto;

internal class Program
{
	private static readonly Stopwatch sStopWatch = new();

	static void Main()
	{
		Game game = Game.Setup( Logic.Samples.GetPlayers(), auto: true, comment: true,
			reverse: true );
		try
		{
			//Game sample = Logic.Samples.Populate();

			sStopWatch.Start();
			if( !game.Play() ) { Console.WriteLine( "Failed to successfully auto-play game." ); }
			else
			{
				sStopWatch.Stop();
				ShowResults( game );
				//HtmlBuilder.CreateHtml( game );
			}
		}
		catch( Exception ex ) { Console.WriteLine( ex.ToString() ); }
		finally
		{
			Console.WriteLine();
			Console.WriteLine( $"Runtime: {sStopWatch.Elapsed} milliseconds" );
			if( Environment.UserInteractive )
			{
				Console.Write( @"Press any key to continue . . . " );
				_ = Console.Read();
			}
		}
	}

	#region Output to Console

	internal static void ShowResults( Game game )
	{
		Console.WriteLine( $"Game target: {game.Target:$#,###,##0} on {game.Date}" );
		Player? winner = game.Winner;
		if( winner is not null )
		{
			Console.WriteLine( $"{winner.Name} won with {winner.Total:$#,###,##0}" );
			Console.WriteLine( $"{winner.Completed.Count} hands played" );
			string reason = game.StackCount == 0 ? "Stack ran out." : "Market closed.";
			Console.WriteLine( $"{winner.Current.Round} rounds played in last hand - {reason}" );
		}
		Console.WriteLine( $"{game.Dealer.Name} was the Dealer" );
		Player? banker = game.GetBanker();
		if( banker is not null ) { Console.WriteLine( $"{banker.Name} was the Banker" ); }
		else { Console.WriteLine( "No Banker" ); }
		Console.WriteLine();

		Console.WriteLine( $"Wasted pile count: {game.WastedPile.Count}" );
		foreach( Card card in game.WastedPile ) { Console.WriteLine( card + card.Comment ); }

		ConsoleColor dftColor = Console.ForegroundColor;
		foreach( Player player in game.PlayOrder )
		{
			Hand? hand = player.Completed.LastOrDefault();
			if( hand is null ) { continue; }
			Console.WriteLine();
			Console.WriteLine( "------------------------------" );
			Console.WriteLine( $"{player}" );

			Console.WriteLine( "-- hassle:" );
			Console.ForegroundColor = ConsoleColor.Red;
			foreach( Card card in hand.HasslePile ) { Console.WriteLine( $"{card} {card.Comment}" ); }
			Console.ForegroundColor = dftColor;

			Console.WriteLine( "-- in hand:" );
			Console.ForegroundColor = ConsoleColor.Yellow;
			foreach( Card card in hand.InHand ) { Console.WriteLine( $"{card} {card.Comment}" ); }
			Console.ForegroundColor = dftColor;

			Console.WriteLine( "-- stash:" );
			Console.ForegroundColor = ConsoleColor.Green;
			foreach( Card card in hand.StashPile ) { Console.WriteLine( $"{card} {card.Comment}" ); }
			Console.ForegroundColor = dftColor;

			Console.WriteLine( "-- score:" );
			Console.WriteLine( $"Protected......: {hand.Protected:###,##0}" );
			Console.WriteLine( $"UnProtected....: {hand.UnProtected:###,##0}" );
			Console.WriteLine( $"Skimmed........: {hand.Skimmed:###,##0}" );
			if( hand.HighestPeddle > 0 ) Console.WriteLine( $"Highest Peddle.: {hand.HighestPeddle:-###,##0}" );
			else Console.WriteLine( $"Highest Peddle.: {hand.HighestPeddle:###,##0}" );
			Console.WriteLine( $"Paranoia Fines.: {hand.ParanoiaFines:###,##0}" );
			Console.WriteLine( $"Net Score......: {hand.NetScore:###,##0}" );
			if( hand.Bonus > 0 ) { Console.WriteLine( $"Win hand Bonus.: {hand.Bonus:###,##0}" ); }
		}
	}

	#endregion
}