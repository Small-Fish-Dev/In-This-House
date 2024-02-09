namespace ITH;

public sealed class Client : Component
{
	[Property] public Connection Connection { get; set; }
	[Property] public GameObject Pawn { get; set; }

	public bool IsLocal => ConnectionId == Connection.Local.Id;
	[Sync] public Guid ConnectionId { get; private set; }
	[Sync] public bool Host { get; private set; }
	[Sync] public string UserName { get; private set; }
	[Sync] public ulong SteamId { get; private set; }
	[Sync] public short Ping { get; private set; }

	public void Connect( Connection channel )
	{
		Connection = channel;
		Host = channel.IsHost;
		ConnectionId = channel.Id;
		UserName = channel.DisplayName;
		SteamId = channel.SteamId;
	}

	protected override void OnStart()
	{
		if ( IsProxy )
			return;

		Local.Client = this;
	}
}
