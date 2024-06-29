namespace Grass.Logic.Models;

/// <summary>Provides the details of a playing card.</summary>
public class Card
{
	/// <summary>Card type information.</summary>
	public CardInfo Info { get; private set; }

	/// <summary>Card type name.</summary>
	public string Name => Info.Name;

	/// <summary>Indicates whether a peddle card is protected in the players stash.</summary>
	public bool Protected { get; set; }

	/// <summary>Card image caption.</summary>
	public string Caption => History.Count == 0 ? string.Empty : History.Last();

	/// <inheritdoc/>
	[System.ComponentModel.EditorBrowsable( System.ComponentModel.EditorBrowsableState.Never )]
	public override string ToString()
	{
		string protect = Protected ? "protected" : string.Empty;
		return Info.Value == 0 ? Name : $"{Name} {Info.Value:###,##0} {protect}".Trim();
	}

	#region Internal Properties and Methods

	internal Card( CardInfo info ) => Info = info;

	internal List<string> History { get; private set; } = [];

	internal void AddComment( string text )
	{
		if( text.Length > 0 ) { History.Add( text ); }
	}

	internal static bool TransferCard( List<Card> from, List<Card> to, Card card )
	{
		bool rtn = from.Remove( card );
		if( !rtn ) { return rtn; }
		to.Add( card );
		return true;
	}

	#endregion
}