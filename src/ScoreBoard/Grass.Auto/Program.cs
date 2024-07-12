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
			//Game sample = Logic.Samples.Populate( endgame: true );

			sStopWatch.Start();
			if( !game.Play() ) { Console.WriteLine( "Failed to successfully auto-play game." ); }
			else
			{
				sStopWatch.Stop();
				Logic.Samples.ShowResults( game );
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
}