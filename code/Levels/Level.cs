using ITH.UI;

namespace ITH;

public enum LevelType
{
	[Icon( "check_box_outline_blank" )]
	None,
	[Icon( "storefront" )]
	Shop,
	[Icon( "house" )]
	Mansion,
	[Icon( "house_siding" )]
	Dungeon,
	[Icon( "wc" )]
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
	}

	protected override void OnDestroy()
	{
	}

	protected override void OnUpdate()
	{
	}

	public async Task Start()
	{
		SetEnabled( true );
		foreach ( var lootSpawner in GameObject.Components.GetAll<LootSpawner>( FindMode.EnabledInSelfAndDescendants ) )
		{
			lootSpawner.SpawnLoot();
		}

		foreach ( var client in MansionGame.Instance.Clients.Values )
		{
			if ( client.Pawn.Components.TryGet<PlayerController>( out var player ) )
			{
				var sp = Random.Shared.FromList( Spawns );
				player.Respawn( sp.Transform.Position );
			}
		}

		await Task.CompletedTask;
	}

	public async Task End()
	{
		// TODO: I just have this off because of dev time lol
		// BlackScreen.Start();
		// await GameTask.DelayRealtimeSeconds( 1f );
		MansionGame.Instance.SetTimerStatus( true );
		SetEnabled( false );

		await Task.CompletedTask;
	}

	private void SetEnabled( bool enabled )
	{
		foreach ( var child in GameObject.GetAllObjects( true ) )
		{
			child.Enabled = enabled;
		}
		Map.GameObject.Enabled = enabled;
	}
}
