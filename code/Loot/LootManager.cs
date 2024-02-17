namespace ITH;

public sealed class LootManager : Component
{
	public static LootManager Instance;

	private List<PrefabDefinition> loot;
	public IReadOnlyList<PrefabDefinition> Loot => loot;

	public LootManager()
	{
		Instance = this;
	}

	protected override void OnDestroy()
	{
		Instance = null;
	}

	protected override void OnAwake()
	{
		loot = PrefabLibrary.FindByComponent<Loot>().ToList();
	}

	// public Loot[] GetAll( Func<Loot, bool> predicate )
	// {

	// }

	// public List<Loot> GetAll( Func<Loot, bool> predicate )
	// {
	// 	List<Loot> components = new();
	// 	foreach ( var x in Loot )
	// 	{
	// 		if ( x.Components.TryGet<Loot>( out var loot, FindMode.EverythingInSelf ) )
	// 		{
	// 			components.Add( loot );
	// 		}
	// 	}
	// 	return components.Where( predicate ).ToList();
	// }
}
