namespace ConsoleApp;

class Program
{
	static void Main( string[] args )
	{
		string sep = new( '-', 80 );
		try
		{
			Reflection.GenericTypes.Show( ref sep );
			Reflection.Utilities.Show( ref sep );
		}
		catch( Exception ex )
		{
			Console.WriteLine( ex.ToString() );
		}
	}
}