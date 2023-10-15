namespace BrickJam;

// THE S&BOX PREFAB EDITOR BROKE LOLOLOL.
[GameResource( "ItemPrefab", "item", "Define item data.", Icon = "stars" )]
public class ItemPrefab : GameResource
{
	public static IReadOnlyDictionary<string, ItemPrefab> All => all;
	private static Dictionary<string, ItemPrefab> all = new();

	public string Name { get; set; }
	public string Description { get; set; }

	[ResourceType( "vmdl" )]
	public string Model { get; set; }

	public ItemRarity Rarity { get; set; }

	// TODO: This could be a function of Rarity? 
	// Common => some_amount
	// Rare => higher_amount
	// Legendary => highest_amount
	public int MonetaryValue { get; set; }

	protected override void PostLoad()
	{
		if ( all.ContainsKey( ResourceName ) )
			return;

		all.Add( ResourceName, this );
	}

	/// <summary>
	/// Gets an ItemPrefab by the ResourceName.
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public static ItemPrefab Get( string name )
	{
		if ( All.TryGetValue( name.ToLower(), out var prefab ) )
			return prefab;

		return null;
	}
}
