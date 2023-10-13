using Sandbox;
using System;
using System.Linq;

namespace BrickJam;

partial class Player : AnimatedEntity
{
	[Net] public float WalkSpeed { get; set; } = 200f;
	[Net] public float RunSpeed { get; set; } = 350f;
	[Net] public float AccelerationSpeed { get; set; } = 300f; // Units per second (Ex. 200f means that after 1 second you've reached 200f speed)
	[Net] public float WishSpeed { get; private set; } = 0f;

	public float StepSize => 16f;
	public float WalkAngle => 46f;

	protected void SimulateController()
	{
		if ( GroundEntity != null )
		{
			WishSpeed = Math.Clamp( WishSpeed + AccelerationSpeed * Time.Delta, 0f, IsRunning ? RunSpeed : WalkSpeed );
			Log.Info( InputDirection );
			Velocity = Vector3.Lerp( Velocity, InputDirection * Rotation.FromYaw( InputAngles.yaw ) * WishSpeed, 15f * Time.Delta )
				.WithZ( Velocity.z );
		}

		if ( Input.Down( "jump" ) )
		{
			if ( GroundEntity != null )
			{
				GroundEntity = null;
				Velocity += Vector3.Up * 300f;
			}
		}

		var helper = new MoveHelper( Position, Velocity );
		helper.MaxStandableAngle = WalkAngle;

		helper.Trace = helper.Trace
			.Size( CollisionBox.Mins, CollisionBox.Maxs )
			.WithoutTags( "player" )
			.Ignore( this );

		helper.TryMoveWithStep( Time.Delta, StepSize );
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
