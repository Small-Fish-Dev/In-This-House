namespace BrickJam;

public partial class LockedComponent : EntityComponent
{
	[Net] public bool Locked { get; set; } = true;

	public void Lockpick()
	{
		if ( !Locked )
			return;

		Game.AssertServer();

		_lockpick( Entity.NetworkIdent );
	}

	public void RequestLockpicked()
		=> _requestLockpicked( Entity.NetworkIdent );

	[ConCmd.Server( "request_lockpicked" )]
	public static void _requestLockpicked( int ident )
	{
		var entity = Entity.FindByIndex( ident );
		if ( entity == null )
			return;

		var component = entity.Components.Get<LockedComponent>();
		if ( component == null )
			return;

		component.Locked = false;
	}

	[ClientRpc]
	public void _lockpick( int ident )
	{
		var entity = Entity.FindByIndex( ident );
		if ( entity == null )
			return;
	
		var component = entity.Components.Get<LockedComponent>();
		if ( component == null )
			return;

		UI.Lockpicker.Open( component );
	}
}
