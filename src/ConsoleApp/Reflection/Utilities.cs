using System.Collections;
using System.Reflection;
using ClassLibrary.Models;

namespace ConsoleApp.Reflection;

public static class Utilities
{
	internal static void Show( ref string sep )
	{
		Console.WriteLine( sep );
		Console.WriteLine( "* Reflection Utility" );
		Console.WriteLine( sep );

		var objType = typeof( List<string> );
		Console.WriteLine( "Is List<string> a list? " + IsList( objType ) );
		Console.WriteLine( "Element type: " + GetCollectionElementType( objType ) );

		Console.WriteLine();
		objType = typeof( List<int> );
		Console.WriteLine( "Is List<int> a list? " + IsList( objType ) );
		Console.WriteLine( "Element type: " + GetCollectionElementType( objType ) );

		Console.WriteLine();
		objType = typeof( Dictionary<string, List<int>> );
		Console.WriteLine( "Is Dictionary<string,List<int>> a dictionary? " + IsDictionary( objType ) );
		Console.WriteLine( "Element type: " + GetCollectionElementType( objType ) );

		Console.WriteLine();
		objType = typeof( Dictionary<string, Movie> );
		Console.WriteLine( "Is Dictionary<string,Movie> a dictionary? " + IsDictionary( objType ) );
		Console.WriteLine( "Element type: " + GetCollectionElementType( objType ) );

		Console.WriteLine();
		Movie movie = new() { Id = 99, Title = "Some title", Description = "Some description", Review = "Some review" };
		PropertyInfo[] props = movie.GetType().GetProperties( BindingFlags.Instance | BindingFlags.Public );
		foreach( PropertyInfo prop in props )
		{
			string name = prop.Name;
			Type propType = prop.PropertyType;
			string? typName, typNamespace;
			if( propType.IsGenericType && propType.GenericTypeArguments.Length > 0 )
			{
				typName = propType.GenericTypeArguments[0].Name;
				typNamespace = propType.GenericTypeArguments[0].Namespace;
			}
			else
			{
				typName = propType.Name;
				typNamespace = propType.Namespace;
			}

			Console.WriteLine( $"Property: {name}, Type: {typNamespace}.{typName}, IsClass: {propType.IsClass}" );
			Console.WriteLine( $"          IsPrimitive: {propType.IsPrimitive}, IsGenericType: {propType.IsGenericType}" );
			Console.WriteLine( $"          IsTypeDefinition: {propType.IsTypeDefinition}, Value: {prop.GetValue( movie )}" );
		}
	}

	#region Private Methods

	private static bool IsList( Type type )
	{
		ArgumentNullException.ThrowIfNull( type );

		if( typeof( IList ).IsAssignableFrom( type ) )
		{
			return true;
		}

		foreach( var it in type.GetInterfaces() )
		{
			if( it.IsGenericType && typeof( IList<> ) == it.GetGenericTypeDefinition() )
			{
				return true;
			}
		}

		return false;
	}

	private static bool IsDictionary( Type type )
	{
		ArgumentNullException.ThrowIfNull( type );

		if( typeof( IDictionary ).IsAssignableFrom( type ) )
		{
			return true;
		}

		foreach( var it in type.GetInterfaces() )
		{
			if( it.IsGenericType && typeof( IDictionary<,> ) == it.GetGenericTypeDefinition() )
			{
				return true;
			}
		}

		return false;
	}

	private static Type? GetCollectionElementType( Type type )
	{
		ArgumentNullException.ThrowIfNull( type );

		// First try the generic way
		// this is easy, just query the IEnumerable<T> interface for its generic parameter
		var enumType = typeof( IEnumerable<> );
		foreach( var bt in type.GetInterfaces() )
		{
			if( bt.IsGenericType && bt.GetGenericTypeDefinition() == enumType )
			{
				return bt.GetGenericArguments()[0];
			}
		}

		// Now try the non-generic way
		// if it's a dictionary we always return DictionaryEntry
		if( typeof( IDictionary ).IsAssignableFrom( type ) )
		{
			return typeof( DictionaryEntry );
		}

		// If it's a list we look for an Item property with an int index parameter
		// where the property type is anything but object
		if( typeof( IList ).IsAssignableFrom( type ) )
		{
			foreach( var prop in type.GetProperties() )
			{
				if( "Item" == prop.Name && typeof( object ) != prop.PropertyType )
				{
					var ipa = prop.GetIndexParameters();
					if( 1 == ipa.Length && typeof( int ) == ipa[0].ParameterType )
					{
						return prop.PropertyType;
					}
				}
			}
		}

		// If it's a collection we look for an Add() method whose parameter is 
		// anything but object
		if( typeof( ICollection ).IsAssignableFrom( type ) )
		{
			foreach( var meth in type.GetMethods() )
			{
				if( "Add" == meth.Name )
				{
					var pa = meth.GetParameters();
					if( 1 == pa.Length && typeof( object ) != pa[0].ParameterType )
					{
						return pa[0].ParameterType;
					}
				}
			}
		}

		return typeof( IEnumerable ).IsAssignableFrom( type ) ? typeof( object ) : null;
	}

	#endregion
}