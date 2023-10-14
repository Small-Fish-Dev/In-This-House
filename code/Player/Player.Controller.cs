using Sandbox;
using System;
using System.Linq;

namespace BrickJam;

public partial class Player : AnimatedEntity
{
	[Net] public float WalkSpeed { get; set; } = 200f;
	[Net] public float RunSpeed { get; set; } = 350f;

	/// <summary>
	/// Acceleration speed in units per second (Ex. 200f means that after 1 second you've reached 200f speed)
	/// </summary>
	[Net]
	public float AccelerationSpeed { get; set; } =
		400f;

	[Net, Predicted] public float WishSpeed { get; private set; } = 0f;

	public float StepSize => 16f;
	public float WalkAngle => 46f;

	protected void SimulateController()
	{
		if ( !InputDirection.IsNearlyZero() )
			WishSpeed = Math.Clamp( WishSpeed + AccelerationSpeed * Time.Delta, 0f, IsRunning ? RunSpeed : WalkSpeed );
		else
			WishSpeed = Math.Clamp( WishSpeed - AccelerationSpeed * Time.Delta, 0f, IsRunning ? RunSpeed : WalkSpeed );

		var oldVelocity = Velocity;
		Velocity = Vector3.Lerp( Velocity,
				(InputDirection.IsNearlyZero()
					? (Velocity.Normal / 3f)
					: (InputDirection.Normal * Rotation.FromYaw( InputAngles.yaw ))) * WishSpeed, 15f * Time.Delta )
			.WithZ( Velocity.z );

		var helper = new MoveHelper( Position, Velocity );
		helper.MaxStandableAngle = WalkAngle;

		helper.Trace = helper.Trace
			.Size( CollisionBox.Mins, CollisionBox.Maxs )
			.WithoutTags( "player" )
			.Ignore( this );

		helper.TryMoveWithStep( Time.Delta, StepSize );
		helper.TryUnstuck();

		// If:
		// - the player is running
		// - the velocity dropped from more than WalkSpeed to near zero
		// - there is a wall in the direction of movement
		// then the pawn has probably ran into a wall
		if ( !Velocity.IsNearZeroLength
		     && IsRunning
		     && oldVelocity.Length > WalkSpeed
		     && helper.Velocity.Length < 15f // TODO: hardcoded
		   )
		{
			var tr = helper.Trace
				.Size( CollisionBox.Mins + Vector3.Up * StepSize, CollisionBox.Maxs )
				.FromTo( helper.Position, helper.Position + Velocity.Normal ).Run();
			if ( tr.Hit )
				Log.Error( "Me after lobotomy!" );
		}

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

		if ( Input.Down( "jump" ) )
			if ( GroundEntity != null )
			{
				GroundEntity = null;
				Velocity += Vector3.Up * 300f;
			}
	}
}
