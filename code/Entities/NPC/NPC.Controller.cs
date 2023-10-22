using GridAStar;

namespace BrickJam;

public partial class NPC
{
	public virtual float WalkSpeed { get; set; } = 120f;
	public virtual float RunSpeed { get; set; } = 380f;
	public Vector3 Direction { get; set; } = Vector3.Zero;
	public float WishSpeed => Direction.IsNearlyZero() ? 0 : ( HasArrivedDestination ? 0f : (Target.IsValid() ? RunSpeed : WalkSpeed) );
	public Vector3 WishVelocity => Direction * WishSpeed;
	public Rotation WishRotation => Rotation.LookAt( Direction, Vector3.Up );
	public bool Blocked { get; set; } = false;

	public virtual void ComputeMotion()
	{
		if ( !Blocked )
			Velocity = Vector3.Lerp( Velocity, WishVelocity, Time.Delta * 15f ).WithZ( Velocity.z );
		else
			Velocity = Vector3.Zero.WithZ( Velocity.z );

		if ( CurrentlyMurdering == null || !CurrentlyMurdering.IsValid() )
			Rotation = Rotation.Lerp( Rotation, WishRotation, Time.Delta * 5f );
		else
			Rotation = Rotation.LookAt( CurrentlyMurdering.Position - Position, Vector3.Up );

		foreach ( var toucher in toPush )
		{
			if ( toucher is not { IsValid: true } )
				continue;

			var direction = (Position - toucher.Position).WithZ( 0 ).Normal;
			var distance = Position.Distance( toucher.Position );

			var pushOffset = direction * MathE.SmoothKernel( CollisionRadius * 2f, distance ) * Time.Delta * 3000f;
			Velocity += pushOffset.WithY( 0 );
		}

		var helper = new MoveHelper( Position, Velocity );
		helper.MaxStandableAngle = 70f;

		helper.Trace = Trace.Capsule( CollisionCapsule, Position, Position );
		helper.Trace = helper.Trace
			.WithoutTags( "player", "npc", "door", "loot", "nocollide" )
			.Ignore( this );

		helper.TryMoveWithStep( Time.Delta, 24f );
		helper.TryUnstuck();

		Position = helper.Position;
		Velocity = helper.Velocity;

		var traceDown = helper.TraceDirection( Vector3.Down );

		if ( traceDown.Entity != null )
		{
			GroundEntity = traceDown.Entity;
			Position = traceDown.EndPosition;

			Velocity = Velocity.WithZ( 0 );
		}
		else
		{
			GroundEntity = null;
			Velocity -= Vector3.Down * Game.PhysicsWorld.Gravity * Time.Delta;
		}
	}
}

