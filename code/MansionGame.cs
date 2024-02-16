namespace ITH;

public sealed partial class MansionGame : Component, Component.INetworkListener, Component.ExecuteInEditor
{
	public static MansionGame Instance;
	[Sync] public TimeUntil TimeOut { get; set; }
	[Sync] public bool TimerActive { get; set; }

	public int Seed { get; set; } = 0;
	public Random Random { get; set; } = new Random();

	public float TimePerLevel => 1800000.0f;

	/// <summary>
	/// Create a server (if we're not joining one)
	/// </summary>
	[Property] public bool StartServer { get; set; } = true;
	[Property] public GameObject PlayerPrefab { get; set; }
	[Property] public List<Level> Levels { get; private set; }
	private List<SpawnPoint> spawnPoints = new();
	public Dictionary<Connection, Client> Clients = new();

	protected override void OnAwake()
	{
		PrefabLibrary.Init();
		if ( Scene.IsEditor )
		{
			return;
		}

		spawnPoints ??= new();
		if ( spawnPoints.Count <= 0 )
		{
			var spawns = Scene.GetAllComponents<SpawnPoint>();
			spawnPoints = spawns.ToList();

			if ( spawnPoints.Count <= 0 )
			{
				var spawn = Scene.CreateObject( true );
				spawn.Transform.Position = Transform.Position + Vector3.Up * 48f;
				var sc = spawn.Components.GetOrCreate<SpawnPoint>();
				spawnPoints[0] = sc;
			}
		}

		Levels = Scene.GetAllComponents<Level>().ToList();

		Instance = this;
		SetLevel( LevelType.Shop );
	}

	protected override void OnDestroy()
	{
		// Need this for the editor, it doesnt clear statics automatically
		// PrefabLibrary.Shutdown();

		Instance = null;
		Local.DestroyAll();
	}

	protected override async Task OnLoad()
	{
		if ( Scene.IsEditor )
			return;

		if ( StartServer && !GameNetworkSystem.IsActive )
		{
			LoadingScreen.Title = "Creating Lobby";
			await Task.DelayRealtimeSeconds( 0.1f );
			GameNetworkSystem.CreateLobby();
		}
	}

	/// <summary>
	/// A client is fully connected to the server. This is called on the host.
	/// </summary>
	public void OnActive( Connection channel )
	{
		Log.Info( $"Player '{channel.DisplayName}' has joined the game" );

		if ( PlayerPrefab is null )
			return;

		// Create a client independent of the gameobject they will control.
		// The client' connection owns this as well as their actual player GameObject.
		var clientGameObject = Scene.CreateObject();
		clientGameObject.Name = $"Client - {channel.DisplayName}";

		var client = clientGameObject.Components.Create<Client>();
		client.Connect( channel );
		clientGameObject.NetworkSpawn( channel );
		Clients.Add( channel, client );

		// Spawn this object and make the client the owner
		var point = Random.Shared.FromList( Levels.First( x => x.Id == LevelType.Shop ).Spawns );
		var player = PlayerPrefab.Clone( point.Transform.Position );
		player.Name = $"Player - {channel.DisplayName}";
		client.Pawn = player;
		player.NetworkSpawn( channel );

	}

	protected override void OnFixedUpdate()
	{
		if ( !Networking.IsHost )
			return;

		if ( TimerActive && TimeOut )
		{
			SetTimerStatus( false );
			RestartGame();
		}
	}

	public void SetTimerStatus( bool enabled, float time = 0f )
	{
		TimerActive = enabled;
		TimeOut = time == 0f ? TimePerLevel : time;
	}

	public void RestartGame()
	{
		ResetRandomSeed();
		Instance.SetLevel( LevelType.Shop );
	}

	public void ResetRandomSeed()
	{
		if ( !Networking.IsHost )
			return;

		Seed = DateTime.UtcNow.Ticks.GetHashCode();
		Random = new Random( Seed );
	}
}
