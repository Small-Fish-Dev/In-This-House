namespace ITH;

public enum LevelType
{
	None,
	Shop,
	Mansion,
	Dungeon,
	Bathrooms
}

public sealed class Level : Component
{
	public static Action<Level> OnLevelChanged;
	[Property] public LevelType Id { get; private set; }
	[Property] public string Music { get; private set; }
	[Property] public Usable Exit { get; set; }
	[Property] public List<SpawnPoint> Spawns { get; private set; }
	[Property] public MapInstance Map { get; private set; }
	[Sync] public NetList<GameObject> Monsters { get; set; }
	[Sync] public TimeSince SinceStarted { get; set; } = 0f;

	protected override void OnAwake()
	{
		Map = GameObject.Components.Get<MapInstance>( FindMode.EverythingInSelfAndChildren );
		Spawns = GameObject.Components.GetAll<SpawnPoint>( FindMode.EnabledInSelfAndDescendants ).ToList();
		OnLevelChanged += HandleLevelChanged;
	}

	protected override void OnDestroy()
	{
		OnLevelChanged -= HandleLevelChanged;
		OnLevelChanged = null;
	}

	protected override void OnUpdate()
	{
	}

	private void HandleLevelChanged( Level upcomingLevel )
	{
		Map.GameObject.Enabled = upcomingLevel.Id == Id;
		Log.Info( $"{Id} : {Map.Enabled}" );
	}
}
