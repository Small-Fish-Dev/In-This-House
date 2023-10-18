using Editor;
using GridAStar;

namespace BrickJam;

public enum DoorState : sbyte
{
	Opening = 1,
	Closing = -1,
	Open = 2,
	Closed = 3,
}

[HammerEntity]
[EditorModel( "models/placeholders/placeholder_door.vmdl" )]
public partial class Door : UsableEntity
{
	[Net] public DoorState State { get; set; } = DoorState.Closed;
	[Net] public bool Locked { get; set; }

	public override string UseString => "open the door";

	private Transform initialTransform;
	private Vector3? _hinge;
	private int side;

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
		Tags.Add( "door" );
		initialTransform = Transform;
	}

	public override void Use( Player user )
	{
		if ( !IsAuthority )
			return;

		if ( Locked )
		{
			// TODO: Open lockpicking UI here.

			return;
		}

		// Set the door state depending on current state.
		State = State == DoorState.Open || State == DoorState.Opening
			? DoorState.Closing
			: DoorState.Opening;

		// I have no clue if this is right or not :DD 
		var angle = Math.Abs( Rotation.LookAt( initialTransform.Position - user.Position ).Yaw() ) 
			+ Math.Abs( initialTransform.Rotation.Yaw() - 90) 
			- 90;

		side = angle > 0 && angle < 180
			? 1
			: -1;
	}

	[GameEvent.Tick]
	void tick()
	{
		// No need to update if we are static.
		if ( State == DoorState.Open || State == DoorState.Closed || !IsAuthority )
			return;

		const float TIME = 0.25f;
		const float ANGLE = 90f;
		const float PUSH_FORCE = 50f;

		// Calculate desired angles.
		var state = (int)State;
		var yaw = initialTransform.Rotation.Yaw()
			+ Math.Max( state, 0 ) * ANGLE * side;

		var targetRotation = initialTransform.Rotation
			.Angles()
			.WithYaw( yaw )
			.ToRotation();

		// Snap the door.
		if ( Rotation.Distance( targetRotation ).AlmostEqual( 0, 1f ) )
		{
			State = State == DoorState.Opening
				? DoorState.Open
				: DoorState.Closed;

			Transform = Transform.WithRotation( targetRotation );

			OccupyCells();
			return;
		}

		// Prevent colliding with players.
		var center = CollisionWorldSpaceCenter;
		var trace = Trace.Body( PhysicsBody, center + Rotation.Left * 4f * state )
			.WithAnyTags( "player" )
			.Run();

		// Push the player if we hit one.
		if ( trace.Hit )
		{
			if ( trace.Entity != null && trace.Entity.IsValid )
				trace.Entity.Velocity += -trace.Normal * PUSH_FORCE;
				
			return;
		}

		// Linear rotation for the door.
		var rot = Rotation.Lerp( Rotation, targetRotation, 1f / TIME * Time.Delta );
		Transform = Transform.WithRotation( rot );
	}

	public void OccupyCells()
	{
		foreach ( var grid in GridAStar.Grid.Grids )
		{
			if ( grid.Value.IsInsideBounds( Position ) )
			{
				for ( var x = -16; x < 16f; x++ )
					for ( var y = -16; y < 16; y++ )
					{
						var checkPos = Position + Rotation.Forward * 5f * x + Rotation.Right * 5f * y + Vector3.Up * 5f;
						var cellFound = grid.Value.GetCell( checkPos );

						if ( cellFound != null )
							cellFound.Tags.Remove( "occupied" );
					}

				for ( var x = 0; x < 15f; x++ )
					for ( var y = -2; y < 3; y++ )
					{
						var checkPos = Position + Rotation.Forward * 5f * x + Rotation.Right * 5f * y + Vector3.Up * 5f;
						var cellFound = grid.Value.GetCell( checkPos );

						if ( cellFound != null )
						{
							cellFound.SetOccupant( this );
							cellFound.Tags.Add( "occupied" );
						}
					}
			}
		}
	}
}
