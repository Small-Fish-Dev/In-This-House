namespace BrickJam;

public partial class Player
{
	/// <summary>
	/// This is the chance of dropping an item when you crash into a wall.
	/// Value should range from 0 to 1.
	/// </summary>
	[Net] public float DropChance { get; set; } = 0.5f;

	[Net] public float CrouchSpeed { get; set; } = 80f;
	[Net] public float WalkSpeed { get; set; } = 200f;
	[Net] public float RunSpeed { get; set; } = 350f;
	[Net] public float JumpHeight { get; set; } = 250f;
	[Net] public float Acceleration { get; set; } = 1200f; // Units per second
	[Net] public float Deceleration { get; set; } = 400f; // Units per second

	public float StunSpeed => (float)(WalkSpeed + (RunSpeed - WalkSpeed) * Math.Sin( 45f.DegreeToRadian() ));
	public float WishSpeed => InputDirection.IsNearlyZero() ? 0 : ( IsCrouching ? CrouchSpeed : (IsRunning ? RunSpeed : WalkSpeed) );
	public Vector3 WishVelocity => ( InputDirection.IsNearlyZero() || IsStunned )
		? Vector3.Zero
		: InputDirection * Rotation.FromYaw( InputAngles.yaw ) * WishSpeed;
	public bool IsAboveWalkingSpeed => Velocity.WithZ( 0 ).Length >= WalkSpeed * 1.1f;

	public float StepSize => 16f;
	public float WalkAngle => 46f;
	public float StunBounceVelocity => 500f;

	static string[] ignoreTags = new[] { "player", "npc", "nocollide", "loot" };
	Sound skiddingSound;
	float baseSkiddingVolume = 0.3f;
	float skiddingVolume;
	TimeSince lastAcceleration;

	protected void SimulateController()
	{
		if ( !MovementLocked )
		{
			if ( WishVelocity.Length >= Velocity.WithZ( 0 ).Length ) // Accelerating
			{
				Velocity += WishVelocity.WithZ( 0 ).Normal * Acceleration * Time.Delta;
				Velocity = Velocity.WithZ( 0 ).ClampLength( WishSpeed ).WithZ( Velocity.z );

				if ( WishVelocity.WithZ( 0 ).Normal.Angle( Velocity.WithZ( 0 ).Normal ) >= 65f )
				{
					if ( IsAboveWalkingSpeed )
						skiddingVolume = Math.Min( skiddingVolume + Time.Delta, baseSkiddingVolume );
				}
				else
					skiddingVolume = Math.Max( skiddingVolume - Time.Delta, 0f );

				lastAcceleration = 0f;
			}
			else // Decelerating
			{
				var momentumCoefficent = Velocity.WithZ( 0 ).Length / WalkSpeed * 1.2f; // Faster you move, more momentum you have, harder to stop
				Velocity = Velocity.WithZ( 0 ).ClampLength( Math.Max( Velocity.WithZ( 0 ).Length - Deceleration / momentumCoefficent * Time.Delta, 0 ) ).WithZ( Velocity.z );

				if ( lastAcceleration >= 0.1f )
				{
					if ( IsAboveWalkingSpeed )
						skiddingVolume = Math.Min( skiddingVolume + Time.Delta, baseSkiddingVolume );
					else
						skiddingVolume = Math.Max( skiddingVolume - Time.Delta, 0f );
				}
			}
		}

		if ( skiddingVolume > 0f )
		{
			if ( !skiddingSound.IsPlaying )
				skiddingSound = PlaySound( "sounds/drift.sound" );
		}
		else
			skiddingSound.Stop();

		skiddingSound.SetVolume( skiddingVolume );

		var helper = new MoveHelper( Position, Velocity );
		helper.MaxStandableAngle = WalkAngle;

		helper.Trace = Trace.Capsule( CollisionCapsule, Position, Position );
		helper.Trace = helper.Trace
			.WithoutTags( ignoreTags )
			.Ignore( this );

		helper.TryMoveWithStep( Time.Delta, StepSize );
		helper.TryUnstuck();

		Position = helper.Position;
		Velocity = helper.Velocity;

		if ( IsAboveWalkingSpeed )
			CalculateStun( helper );

		if ( !IsCrouching )
		{
			CalculateTrip( helper );
			CalculateSlip( helper );
		}

		var traceDown = helper.TraceDirection( Vector3.Down );

		if ( traceDown.Entity != null )
		{
			if ( GroundEntity == null )
				PlayLandSound();

			GroundEntity = traceDown.Entity;
			Position = traceDown.EndPosition;
			Velocity = Velocity.WithZ( 0 );
		}
		else
		{
			GroundEntity = null;
			Velocity -= Vector3.Down * Game.PhysicsWorld.Gravity * Time.Delta;
		}

		if ( !CommandsLocked )
		{
			if ( Input.Pressed( "jump" ))
				if ( GroundEntity != null )
				{
					GroundEntity = null;
					Velocity += Vector3.Up * JumpHeight;
					PlayJumpSound();
				}
		}

	}

	protected void CalculateStun( MoveHelper helper )
	{
		// If:
		// - the player is running
		// - the velocity dropped from more than WalkSpeed to near zero
		// - there is a wall in the direction of movement
		// then the pawn has probably ran into a wall
		if ( !IsStunned &&
			 !Velocity.WithZ( 0 ).IsNearZeroLength
			 && Velocity.WithZ( 0 ).Length > WalkSpeed
		   )
		{
			var higherCapsule = CollisionCapsule;
			higherCapsule.CenterA = Vector3.Up * (CollisionRadius + StepSize);
			helper.Trace = Trace.Capsule( higherCapsule, helper.Position, helper.Position + helper.Velocity.WithZ( 0 ) * Time.Delta );
			helper.Trace = helper.Trace
				.WithoutTags( ignoreTags )
				.Ignore( this );
			var tr = helper.Trace.Run();

			if ( tr.Hit )
			{
				var dotProduct = Math.Abs( Vector3.Dot( Velocity.WithZ( 0 ).Normal, tr.Normal ) );
				var wallVelocity = Velocity.WithZ( 0 ) * dotProduct;

				if ( wallVelocity.Length > WalkSpeed )
				{
					var difference = MathX.Remap( wallVelocity.Length, WalkSpeed, RunSpeed );
					Stun( difference );

					Velocity += tr.Normal * (CollisionRadius + StunBounceVelocity);
					Rotation = Rotation.LookAt( -tr.Normal, Vector3.Up );

					// Let's randomly throw out an item when we crash.
					if ( Game.IsServer )
					{
						var random = MansionGame.Random.Float( 0f, 1f );
						if ( random < DropChance )
							ThrowRandomLoot();
					}
				}
			}
		}
	}

	protected void CalculateTrip( MoveHelper helper )
	{
		// If:
		// - the player is running
		// - the velocity dropped from more than WalkSpeed to near zero
		// - there is a wall in the direction of movement
		// then the pawn has probably ran into a wall
		if ( !IsTripping &&
			 !Velocity.WithZ( 0 ).IsNearZeroLength
			 && Velocity.WithZ( 0 ).Length > WalkSpeed
		   )
		{
			var lowerCapsule = CollisionCapsule;
			lowerCapsule.CenterB = Vector3.Up * (StepSize - CollisionRadius);
			helper.Trace = Trace.Capsule( lowerCapsule, helper.Position, helper.Position + helper.Velocity.WithZ( 0 ) * Time.Delta );
			helper.Trace = helper.Trace
				.WithTag( "loot" )
				.Ignore( this );
			var tr = helper.Trace.Run();

			if ( tr.Hit )
				Trip();
		}
	}

	protected void CalculateSlip( MoveHelper helper )
	{
		// If:
		// - the player is running
		// - the velocity dropped from more than WalkSpeed to near zero
		// - there is a wall in the direction of movement
		// then the pawn has probably ran into a wall
		if ( !IsSlipping &&
			 !Velocity.WithZ( 0 ).IsNearZeroLength
			 && Velocity.WithZ( 0 ).Length > WalkSpeed
		   )
		{
			var lowerCapsule = CollisionCapsule;
			lowerCapsule.CenterB = Vector3.Up * (StepSize - CollisionRadius);
			helper.Trace = Trace.Capsule( lowerCapsule, helper.Position, helper.Position + helper.Velocity.WithZ( 0 ) * Time.Delta );
			helper.Trace = helper.Trace
				.WithTag( "slip" )
				.Ignore( this );
			var tr = helper.Trace.Run();

			if ( tr.Hit )
				Slip();
		}
	}

	protected void ThrowRandomLoot()
	{
		var items = Inventory.Loots
			.Keys
			.ToList();

		var randomLoot = MansionGame.Random.FromList( items );
		if ( randomLoot == null || !Inventory.Remove( randomLoot.Value ) )
			return;

		var force = 300f;
		var normal = Vector3.Random.WithZ( 0 );
		var entity = Loot.CreateFromEntry( 
			randomLoot.Value, 
			Position + Vector3.Up * CollisionBounds.Maxs.z / 2f, 
			Rotation.FromYaw( Rotation.Inverse.Yaw() ) 
		);
		entity.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
		entity.ApplyAbsoluteImpulse( (normal + Vector3.Up) * force );
	}
}
