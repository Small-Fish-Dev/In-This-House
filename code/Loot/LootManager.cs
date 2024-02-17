namespace ITH;

public sealed class LootManager : Component
{
	public static LootManager Instance;

	private List<PrefabDefinition> loot;
	public IReadOnlyList<PrefabDefinition> Loot => loot;
	public List<PrefabDefinition> MansionLoot { get; private set; }

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
		MansionLoot = loot.Where( x => x.GetComponent<Loot>().Get<LevelType>( "LevelCanAppearOn" ) == LevelType.Mansion ).ToList();
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
