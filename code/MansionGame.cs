namespace ITH;

public sealed class MansionGame : Component, Component.INetworkListener
{
	/// <summary>
	/// Create a server (if we're not joining one)
	/// </summary>
	[Property] public bool StartServer { get; set; } = true;
	[Property] public GameObject PlayerPrefab { get; set; }
	private List<SpawnPoint> spawnPoints = new();

	protected override void OnAwake()
	{
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
		// Clients.Add( channel, client );

		// Spawn this object and make the client the owner
		var point = Random.Shared.FromList( spawnPoints );
		var player = PlayerPrefab.Clone( point.Transform.Position );
		player.Name = $"Player - {channel.DisplayName}";
		client.Pawn = player;
		player.NetworkSpawn( channel );
	}
}
