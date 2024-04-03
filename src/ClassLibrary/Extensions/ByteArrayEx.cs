using System.Text;

namespace ClassLibrary.Extensions;

public static class ByteArrayEx
{
	public static string ToHexString( this byte[] hex )
	{
		if( hex is null || hex.Length == 0 ) { return string.Empty; }

		StringBuilder sb = new();
		foreach( byte b in hex )
		{
			sb.Append( b.ToString( "x2" ) );
		}
		return sb.ToString();
	}
}