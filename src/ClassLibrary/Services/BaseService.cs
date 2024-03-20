global using ClassLibrary.Models;
using System.Text.Json;

namespace ClassLibrary;

public abstract class BaseService
{
	protected static async Task<List<Movie>> GetMovieDataAsync()
	{
		FileInfo? fi = CheckFile( "Data\\Movies.json" );
		if( fi is null || !fi.Exists ) { return []; }

		string? json = await File.ReadAllTextAsync( fi.FullName );
		return DeserializeList<Movie>( ref json ).ToList();
	}

	#region Private Methods

	private static FileInfo? CheckFile( string fileName )
	{
		string? path = Path.GetDirectoryName( System.Reflection.Assembly.GetExecutingAssembly().Location );
		if( path is null ) { return null; }

		FileInfo fi = new( Path.Combine( path, fileName ) );
		return !fi.Exists ? null : fi;
	}

	private static List<T> DeserializeList<T>( ref string? json, JsonSerializerOptions? options = null )
	{
		if( json is not null )
		{
			options ??= DefaultSerializerOptions();
			try
			{
				List<T>? obj = JsonSerializer.Deserialize<List<T>>( json, options );
				if( obj is not null ) { return obj; }
			}
			catch( ArgumentException ) { }
			catch( JsonException ) { }
			catch( NotSupportedException ) { }
		}

		return [];
	}

	private static JsonSerializerOptions DefaultSerializerOptions()
	{
		JsonSerializerOptions rtn = new()
		{
			AllowTrailingCommas = true,
			IgnoreReadOnlyProperties = true,
			MaxDepth = 6,
			PropertyNameCaseInsensitive = true,
			NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
			Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create( System.Text.Unicode.UnicodeRanges.BasicLatin ),
		};
		return rtn;
	}

	#endregion
}