using GridAStar;

namespace BrickJam;

public partial class NPC
{
	public virtual float WalkSpeed { get; set; } = 140f;
	public virtual float RunSpeed { get; set; } = 400f;
	public virtual float Acceleration { get; set; } = 1000f; // Units per second
	public virtual float Deceleration { get; set; } = 2600f; // Units per second
	public Vector3 Direction { get; set; } = Vector3.Zero;
	public float WishSpeed => Direction.IsNearlyZero() ? 0 : ( HasArrivedDestination ? 0f : (Target.IsValid() ? RunSpeed : WalkSpeed) );
	public Vector3 WishVelocity => Direction * WishSpeed;
	public Rotation WishRotation => Rotation.LookAt( Direction, Vector3.Up );
	public bool Blocked { get; set; } = false;

	public virtual void ComputeMotion()
	{
		if ( !Blocked )
		{
			if ( WishVelocity.Length >= Velocity.WithZ( 0 ).Length ) // Accelerating
			{
				Velocity += WishVelocity.WithZ( 0 ).Normal * Acceleration * Time.Delta;
				Velocity = Velocity.WithZ( 0 ).ClampLength( WishSpeed ).WithZ( Velocity.z );
			}
			else // Decelerating
			{
				var momentumCoefficent = Velocity.WithZ( 0 ).Length / WalkSpeed * 1.2f; // Faster you move, more momentum you have, harder to stop
				Velocity = Velocity.WithZ( 0 ).ClampLength( Math.Max( Velocity.WithZ( 0 ).Length - Deceleration / momentumCoefficent * Time.Delta, 0 ) ).WithZ( Velocity.z );
			}
		}
		else
			Velocity = Vector3.Zero.WithZ( Velocity.z );

		if ( CurrentlyMurdering == null )
			Rotation = Rotation.Lerp( Rotation, WishRotation, Time.Delta * 5f );
		else
			Rotation = Rotation.LookAt( CurrentlyMurdering.Position - Position, Vector3.Up );

		var helper = new MoveHelper( Position, Velocity );
		helper.MaxStandableAngle = 70f;

		helper.Trace = Trace.Capsule( CollisionCapsule, Position, Position );
		helper.Trace = helper.Trace
			.WithoutTags( "player", "npc" )
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

