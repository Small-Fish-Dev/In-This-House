namespace BrickJam;

public partial class LockedComponent : EntityComponent
{
	[Net] public bool Locked { get; set; } = true;
	[Net] public AnimatedEntity Lock { get; set; }	

	public LockedComponent() 
	{
		// Lock model!
		if ( Game.IsServer )
		{
			Lock = new();
			Lock.SetModel( "models/items/lock/lock.vmdl" );
			Lock.SetParent( Entity, "lock" );
			Lock.Transmit = TransmitType.Always;
		}
	}

	public void Lockpick( Entity target )
	{
		if ( !Locked )
			return;

		Game.AssertServer();

		if ( target.Client != null )
			_lockpick( To.Single( target.Client ), Entity.NetworkIdent );
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
		
		component.Lock.SetAnimParameter( "unlocked", true );
		component.Lock.Tags.Add( "nocollide" );
		component.Lock.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
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
