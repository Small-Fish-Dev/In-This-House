namespace BrickJam;

[GameResource( "LootPresets", "lootpres", "Define lists of loots", Icon = "stars" )]
public class LootPresets : GameResource
{
	public static IReadOnlyDictionary<string, LootPresets> All => all;
	private static Dictionary<string, LootPresets> all = new();

	public IList<Loot> Loot { get; set; }

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
	public static LootPresets Get( string name )
	{
		if ( All.TryGetValue( name.ToLower(), out var prefab ) )
			return prefab;

		return null;
	}
}
