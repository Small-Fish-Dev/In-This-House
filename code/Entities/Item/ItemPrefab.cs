namespace BrickJam;

[GameResource( "ItemPrefab", "item", "Define item data.", Icon = "stars" )]
public class ItemPrefab : GameResource
{
	public static IReadOnlyDictionary<string, ItemPrefab> All => all;
	private static Dictionary<string, ItemPrefab> all = new();

	public string Name { get; set; }

	[ResourceType( "vmdl" )]
	public string Model { get; set; }
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
