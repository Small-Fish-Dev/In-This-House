using Editor;
namespace BrickJam;

public enum DoorState
{
	Open,
	Closed,
	Opening,
	Closing
}

[HammerEntity]
[EditorModel( "models/placeholders/placeholder_door.vmdl" )]
public partial class Door : UseableEntity
{
	[Net] public DoorState State { get; set; }
	[Net] public bool Locked { get; set; }

	private Transform initialTransform;
	private Vector3? _hinge;

	private Vector3 hinge
	{
		get
		{
			if ( _hinge == null )
				_hinge = Position.WithZ( WorldSpaceBounds.Center.z ) + CollisionBounds.Maxs.WithZ( 0 );

			return _hinge.Value;
		}
	}

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/placeholders/placeholder_door.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		initialTransform = Transform;
	}

	public override void Use( Player user )
	{
		base.Use( user );

		if ( Locked )
		{
			// Do we have a lockpick?
			var lockpick = ItemPrefab.Get( "lockpick" );
			if ( user.Inventory.Has( lockpick ) > 1 )
			{
				// TODO: Open lockpicking UI here.
			}

			return;
		}

		// Set the door state depending on current state.
		State = State == DoorState.Open 
			? DoorState.Closing
			: State == DoorState.Closed
				? DoorState.Opening
				: State;
	}

	[GameEvent.Tick]
	void tick()
	{
		// No need to update if we are static.
		if ( State == DoorState.Open || State == DoorState.Closed )
			return;

		const float TIME = 0.25f;
		const float ANGLE = 90f;
		const float PUSH_FORCE = 100f;

		var direction = State == DoorState.Opening
			? 1
			: -1;

		// Check if we're already at the desired yaw.
		var inverse = Rotation * initialTransform.Rotation.Inverse;
		var targetRotation = initialTransform.Rotation
			.Angles()
			.WithYaw( direction == 1
				? ANGLE
				: 0 )
			.ToRotation();

		// Just reset the door.
		if ( inverse.Distance( targetRotation ).AlmostEqual( 0, 1f ) )
		{
			State = State == DoorState.Opening
				? DoorState.Open
				: DoorState.Closed;

			Transform = initialTransform;
			Transform = Transform.RotateAround( hinge, targetRotation );

			return;
		}

		// Prevent colliding with players.
		var trace = Trace.Body( PhysicsBody, Position + Rotation.Right * 4f * direction )
			.WithAnyTags( "player" )
			.Run();

		// Push the player if we hit one.
		if ( trace.Hit )
		{
			if ( trace.Entity != null && trace.Entity.IsValid )
				trace.Entity.Velocity += (trace.StartPosition - trace.Entity.Position).Normal * PUSH_FORCE;

			return;
		}

		// Rotate around the hinge.
		var rot = Rotation.Lerp( inverse, targetRotation, 1f / TIME * Time.Delta );
		Transform = initialTransform;
		Transform = Transform.RotateAround( hinge, rot );
	}
}
