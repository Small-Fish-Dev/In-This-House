namespace BrickJam;

public partial class Player
{
	/// <summary>
	/// This is the chance of dropping an item when you crash into a wall.
	/// Value should range from 0 to 1.
	/// </summary>
	[Net] public float DropChance { get; set; } = 0.5f;

	[Net] public float WalkSpeed { get; set; } = 200f;
	[Net] public float RunSpeed { get; set; } = 350f;
	[Net] public float Friction { get; set; } = 0.7f; // The lower the more you slip 

	public float StunSpeed => (float)(WalkSpeed + (RunSpeed - WalkSpeed) * Math.Sin( 45f.DegreeToRadian() ));
	[Net, Predicted] public float WishSpeed { get; private set; } = 0f;

	public float StepSize => 16f;
	public float WalkAngle => 46f;
	public float StunBounceVelocity => 100f;

	protected void SimulateController()
	{
		WishSpeed = IsRunning ? RunSpeed : WalkSpeed;

		var oldVelocity = Velocity;
		var wishVelocity = (InputDirection.IsNearlyZero() || IsStunned
			? Vector3.Zero
			: InputDirection.Normal * Rotation.FromYaw( InputAngles.yaw )) * WishSpeed;

		var velocityDot = Vector3.Dot( wishVelocity.WithZ( 0 ).Normal, oldVelocity.WithZ(0).Normal );
		var velocityDifference = MathX.Remap( velocityDot, 0.8f, 1f, 0.1f, 1f );
		var sideSlipper = Velocity.Length <= WalkSpeed ? 1 : velocityDifference;

		var lerpVelocity = Vector3.Lerp( Velocity, wishVelocity,
			(wishVelocity.LengthSquared > Velocity.LengthSquared ? 15f * sideSlipper : 5f * Friction / ( Velocity.Length / RunSpeed ) ) // Accelerate faster than decelerate
			* Time.Delta )
		.WithZ( Velocity.z );

		var helper = new MoveHelper( Position, lerpVelocity );
		helper.MaxStandableAngle = WalkAngle;

		helper.Trace = Trace.Capsule( CollisionCapsule, Position, Position );
		helper.Trace = helper.Trace
			.WithoutTags( "player", "npc" )
			.Ignore( this );

		helper.TryMoveWithStep( Time.Delta, StepSize );
		helper.TryUnstuck();

		Position = helper.Position;
		Velocity = helper.Velocity;

		// If:
		// - the player is running
		// - the velocity dropped from more than WalkSpeed to near zero
		// - there is a wall in the direction of movement
		// then the pawn has probably ran into a wall
		if ( !IsStunned &&
		     !lerpVelocity.WithZ( 0 ).IsNearZeroLength
		     && oldVelocity.WithZ( 0 ).Length > WalkSpeed
		     && helper.Velocity.WithZ( 0 ).Length < StunSpeed
		   )
		{
			var higherCapsule = CollisionCapsule;
			higherCapsule.CenterA = Vector3.Up * (CollisionRadius + StepSize);
			helper.Trace = Trace.Capsule( higherCapsule, helper.Position, helper.Position + lerpVelocity.WithZ( 0 ).Normal );
			helper.Trace = helper.Trace
			.WithoutTags( "player", "npc" )
			.Ignore( this );
			var tr = helper.Trace.Run();

			if ( tr.Hit )
			{
				var dotProduct = Math.Abs( Vector3.Dot( lerpVelocity.WithZ(0).Normal, tr.Normal ) );
				var wallVelocity = lerpVelocity.WithZ(0) * dotProduct;

				if ( wallVelocity.Length > WalkSpeed )
				{
					var difference = MathX.Remap( wallVelocity.Length, WalkSpeed, RunSpeed );
					Stun( difference );
					Velocity += tr.Normal * (CollisionRadius + StunBounceVelocity);

					// Let's randomly throw out an item when we crash.
					if ( Game.IsServer )
					{
						var random = Game.Random.Float( 0f, 1f );
						if ( random < DropChance )
							ThrowRandomLoot();
					}
				}
			}
		}

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

		if ( Input.Down( "jump" ) && !IsStunned )
			if ( GroundEntity != null )
			{
				GroundEntity = null;
				Velocity += Vector3.Up * 300f;
			}
	}

	protected void ThrowRandomLoot()
	{
		var items = Inventory.Loots
			.Keys
			.ToList();

		var randomLoot = Game.Random.FromList( items );
		if ( randomLoot == null || !Inventory.Remove( randomLoot.Value ) )
			return;

		var force = 300f;
		var normal = Vector3.Random.WithZ( 0 );
		var entity = Loot.CreateFromEntry( 
			randomLoot.Value, 
			Position + Vector3.Up * CollisionBounds.Maxs.z / 2f, 
			Rotation.FromYaw( Rotation.Inverse.Yaw() ) 
		);

		entity.ApplyAbsoluteImpulse( (normal + Vector3.Up) * force );
	}
}
