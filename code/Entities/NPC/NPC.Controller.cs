using GridAStar;

namespace BrickJam;

public partial class NPC
{
	public virtual float WalkSpeed { get; set; } = 120f;
	public virtual float RunSpeed { get; set; } = 250f;
	public Vector3 Direction { get; set; } = Vector3.Zero;

	public virtual void ComputeMotion()
	{
		if ( Direction.IsNearZeroLength ) return;

		var speed = HasArrivedDestination ? 0f : ( Target != null ? RunSpeed : WalkSpeed );

		Rotation = Rotation.Lerp( Rotation, Rotation.LookAt( Direction, Vector3.Up ), Time.Delta * 5f );

		var helper = new MoveHelper( Position, Rotation.Forward * speed );
		helper.MaxStandableAngle = 60f;

		helper.Trace = helper.Trace
			.Size( CollisionBox.Mins, CollisionBox.Maxs )
			.WithoutTags( "player" )
			.Ignore( this );

		helper.TryMoveWithStep( Time.Delta, 20f );
		helper.TryUnstuck();

		Position = helper.Position;
		Velocity = helper.Velocity;

		var traceDown = helper.TraceDirection( Vector3.Down );

		if ( traceDown.Entity != null )
		{
			GroundEntity = traceDown.Entity;
			Position = traceDown.EndPosition;
		}
		else
		{
			GroundEntity = null;
			Velocity -= Vector3.Down * Game.PhysicsWorld.Gravity * Time.Delta;
		}
	}
}

