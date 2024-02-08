namespace ITH;

public sealed class MansionGame : Component, Component.INetworkListener
{
	/// <summary>
	/// Create a server (if we're not joining one)
	/// </summary>
	[Property] public bool StartServer { get; set; } = true;
	[Property] public GameObject PlayerPrefab { get; set; }

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


		// Spawn this object and make the client the owner
		var player = PlayerPrefab.Clone( Transform.Position + Vector3.Up * 8 );
		player.Name = $"Player - {channel.DisplayName}";
		player.NetworkSpawn( channel );
	}
}
