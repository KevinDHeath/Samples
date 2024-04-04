using System.Net;
using System.Runtime.InteropServices;
using System.Security;

namespace ClassLibrary.Extensions;

// https://itecnote.com/tecnote/c-compare-two-securestrings-for-equality/
public static class SecureStringEx
{
	public static bool IsEqualTo( this SecureString ss1, SecureString ss2 )
	{
		IntPtr bstr1 = IntPtr.Zero;
		IntPtr bstr2 = IntPtr.Zero;
		try
		{
			bstr1 = Marshal.SecureStringToBSTR( ss1 );
			bstr2 = Marshal.SecureStringToBSTR( ss2 );
			int length1 = Marshal.ReadInt32( bstr1, -4 );
			int length2 = Marshal.ReadInt32( bstr2, -4 );
			if( length1 == length2 )
			{
				for( int x = 0; x < length1; ++x )
				{
					byte b1 = Marshal.ReadByte( bstr1, x );
					byte b2 = Marshal.ReadByte( bstr2, x );
					if( b1 != b2 ) return false;
				}
			}
			else return false;
			return true;
		}
		finally
		{
			if( bstr2 != IntPtr.Zero ) Marshal.ZeroFreeBSTR( bstr2 );
			if( bstr1 != IntPtr.Zero ) Marshal.ZeroFreeBSTR( bstr1 );
		}
	}

	#region Alternate Secure String comparisons

	// https://www.sjoerdlangkemper.nl/2017/11/08/comparing-securestrings-in-dotnet/
	// https://github.com/Sjord/CompareSecureStrings
	internal static string? SecureStringToString( SecureString value )
	{
		IntPtr valuePtr = IntPtr.Zero;
		try
		{
			valuePtr = Marshal.SecureStringToGlobalAllocUnicode( value );
			return Marshal.PtrToStringUni( valuePtr );
		}
		finally
		{
			Marshal.ZeroFreeGlobalAllocUnicode( valuePtr );
		}
	}

	public static bool IsEqual( SecureString ss1, SecureString ss2 )
	{
		var bstr1 = Marshal.SecureStringToBSTR( ss1 );
		var bstr2 = Marshal.SecureStringToBSTR( ss2 );

		var result = IsEqual( bstr1, bstr2 );

		Marshal.ZeroFreeBSTR( bstr2 );
		Marshal.ZeroFreeBSTR( bstr1 );

		return result;
	}

	private static bool IsEqual( IntPtr bstr1, IntPtr bstr2 )
	{
		var length1 = Marshal.ReadInt32( bstr1, -4 );
		var length2 = Marshal.ReadInt32( bstr2, -4 );

		if( length1 != length2 ) return false;

		var equal = 0;
		for( var i = 0; i < length1; i++ )
		{
			var c1 = Marshal.ReadByte( bstr1 + i );
			var c2 = Marshal.ReadByte( bstr2 + i );
			equal |= c1 ^ c2;
		}
		return equal == 0;
	}

	#endregion
}