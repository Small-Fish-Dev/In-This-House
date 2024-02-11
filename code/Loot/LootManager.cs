namespace ITH;

public sealed class LootManager : Component
{
	public static LootManager Instance;
	[Property] private List<GameObject> Prefabs { get; set; }

	// sbox moment
	private List<GameObject> Loot = new();

	public LootManager()
	{
		Instance = this;
	}

	protected override void OnAwake()
	{
		foreach ( var x in Prefabs )
		{
			var clone = x.Clone( GameObject, Vector3.Zero, Rotation.Identity, Vector3.One );
			clone.BreakFromPrefab();
			clone.Tags.Add( ITH.Tag.HackPrefab );
			Loot.Add( clone );
		}
		Log.Info( Loot.Count );
	}

	protected override void OnDestroy()
	{
		Instance = null;
	}

	public List<Loot> GetAll( Func<Loot, bool> predicate )
	{
		List<Loot> components = new();
		foreach ( var x in Loot )
		{
			if ( x.Components.TryGet<Loot>( out var loot, FindMode.EverythingInSelf ) )
			{
				components.Add( loot );
			}
		}
		return components.Where( predicate ).ToList();
	}
}
