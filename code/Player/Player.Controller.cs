﻿namespace BrickJam;

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
	[Net] public float Acceleration { get; set; } = 1000f;
	[Net] public float Deceleration { get; set; } = 400f;
	public bool HasFrictionUpgrade => HasUpgrade( "Work Shoes" );

	public float StunSpeed => (float)(WalkSpeed + (RunSpeed - WalkSpeed) * Math.Sin( 45f.DegreeToRadian() ));
	public float WishSpeed => InputDirection.IsNearlyZero() ? 0 : ( IsCrouching ? CrouchSpeed : (IsRunning ? RunSpeed : WalkSpeed) );
	public Vector3 WishVelocity => ( InputDirection.IsNearlyZero() || IsStunned )
		? Vector3.Zero
		: InputDirection * Rotation.FromYaw( InputAngles.yaw ) * WishSpeed;
	public bool IsAboveWalkingSpeed => Velocity.WithZ( 0 ).Length >= MathX.Lerp( WalkSpeed, RunSpeed, 0.5f );

	public float StepSize => 16f;
	public float WalkAngle => 70f;
	public float StunBounceVelocity => 550f;

	static string[] ignoreTags = new[] { "player", "npc", "nocollide", "loot" };
	Sound skiddingSound;
	float baseSkiddingVolume = 0.3f;
	float skiddingVolume;
	TimeSince lastAcceleration;
	internal Particles skiddingParticle { get; set; }

	protected void SimulateController()
	{
		if ( !MovementLocked )
		{
			if ( WishVelocity.Length >= Velocity.WithZ( 0 ).Length ) // Accelerating
			{
				var momentumCoefficent = Velocity.WithZ( 0 ).Length / WalkSpeed * 50f; // Slower you move, less momentum you have, easier to start
				Velocity += WishVelocity.WithZ( 0 ).Normal * ( Acceleration - momentumCoefficent ) * ( HasFrictionUpgrade ? 3f : 1f ) * Time.Delta;
				Velocity = Velocity.WithZ( 0 ).ClampLength( WishSpeed ).WithZ( Velocity.z );

				if ( WishVelocity.WithZ( 0 ).Normal.Angle( Velocity.WithZ( 0 ).Normal ) >= 65f )
				{
					if ( IsAboveWalkingSpeed )
						skiddingVolume = Math.Min( skiddingVolume + Time.Delta, baseSkiddingVolume );
					else
						skiddingVolume = Math.Max( skiddingVolume - Time.Delta, 0f );
				}
				else
					skiddingVolume = Math.Max( skiddingVolume - Time.Delta, 0f );

				lastAcceleration = 0f;
			}
			else // Decelerating
			{
				var momentumCoefficent = Velocity.WithZ( 0 ).Length / WalkSpeed * 1.2f; // Faster you move, more momentum you have, harder to stop
				Velocity = Velocity.WithZ( 0 ).ClampLength( Math.Max( Velocity.WithZ( 0 ).Length - (Deceleration * (HasFrictionUpgrade ? 3f : 1f)) / momentumCoefficent * Time.Delta, 0 ) ).WithZ( Velocity.z );

				if ( lastAcceleration >= 0.1f )
				{
					if ( IsAboveWalkingSpeed )
						skiddingVolume = Math.Min( skiddingVolume + Time.Delta, baseSkiddingVolume );
					else
						skiddingVolume = Math.Max( skiddingVolume - Time.Delta, 0f );
				}
				else
					skiddingVolume = Math.Max( skiddingVolume - Time.Delta, 0f );
			}
		}
		else
			skiddingVolume = Math.Max( skiddingVolume - Time.Delta, 0f );

		if ( Blocked )
			Velocity = Vector3.Zero.WithZ( Velocity.z );

		if ( skiddingVolume > 0f )
		{
			if ( !skiddingSound.IsPlaying )
				skiddingSound = PlaySound( "sounds/drift.sound" );

			if ( skiddingParticle == null )
				skiddingParticle = Particles.Create( "particles/dust/dust.vpcf", this, true );
		}
		else
		{
			skiddingSound.Stop();
			skiddingParticle?.Destroy();
		}

		skiddingSound.SetVolume( skiddingVolume );

		foreach ( var toucher in toPush )
		{
			if ( toucher is not { IsValid: true } )
				continue;

			var direction = (Position - toucher.Position).WithZ( 0 ).Normal;
			var distance = Position.Distance( toucher.Position );

			var pushOffset = direction * MathE.SmoothKernel( CollisionRadius * 2f, distance ) * Time.Delta * PushForce;
			Velocity += pushOffset.WithY( 0 );
		}

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
		{
			CalculateStun( helper );
			CalculateTrip( helper );
		}

		if ( !IsCrouching )
			CalculateSlip( helper );

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
				.WithoutTags( "nocollide", "loot", "doob" )
				.Ignore( this );
			var tr = helper.Trace.Run();

			if ( tr.Hit )
			{
				var dotProduct = Math.Abs( Vector3.Dot( Velocity.WithZ( 0 ).Normal, tr.Normal ) );
				var wallVelocity = Velocity.WithZ( 0 ) * dotProduct;
				var difference = MathX.Remap( wallVelocity.Length, WalkSpeed, RunSpeed );

				if ( wallVelocity.Length > WalkSpeed )
				{
					if ( tr.Entity is Player other )
					{
						difference /= 2f;

						if ( Game.IsServer )
						{
							var random = MansionGame.Random.Float( 0f, 1f );
							if ( random < DropChance )
								other.ThrowRandomLoot();

							other.Stun( difference );

							other.Velocity -= tr.Normal * (CollisionRadius + StunBounceVelocity - Velocity.WithZ(0).Length);
							other.Rotation = Rotation.LookAt( tr.Normal, Vector3.Up );
						}
					}

					Stun( difference );

					if ( Game.IsServer )
					{
						Particles.Create( "particles/smoke/smoke_impact.vpcf", Position + Rotation.Forward * CollisionRadius + Rotation.Up * CollisionHeight / 2f );
						var impact = Particles.Create( "particles/impact/impact.vpcf", Position + Rotation.Forward * CollisionRadius + Rotation.Up * CollisionHeight / 2f );

						impact.SetForward( 1, tr.Normal );
					}

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
			 && IsAboveWalkingSpeed
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
			 && Velocity.WithZ( 0 ).Length > CrouchSpeed
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

		Eventlog.Send( $"Whoops... You slipped and dropped <gray>1x {randomLoot?.Name}.", To.Single( Client ) );

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
