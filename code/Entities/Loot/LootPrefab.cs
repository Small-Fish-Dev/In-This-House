namespace BrickJam;

[GameResource( "LootPrefab", "loot", "Define loot data.", Icon = "star" )]
public class LootPrefab : GameResource
{
	public static IReadOnlyDictionary<string, LootPrefab> All => all;
	private static Dictionary<string, LootPrefab> all = new();

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
	/// Gets an LootPrefab by the ResourceName.
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public static LootPrefab Get( string name )
	{
		if ( All.TryGetValue( name.ToLower(), out var prefab ) )
			return prefab;

		return null;
	}
}
