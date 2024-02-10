namespace ITH;

partial class PlayerController
{
	public bool IsAlive { get; private set; } = true;
	[Sync] public bool Blocked { get; set; } = false;
	public bool CommandsLocked => IsStunned || MovementLocked || Blocked;
	public bool MovementLocked => IsTripping || IsSlipping || !IsAlive;

	public float StunDuration { get; set; } = 1.5f;
	public bool IsStunned => !StunLeft;
	[Sync( Query = true )] public TimeUntil StunLeft { get; set; }

	public void Stun( float multiplier = 1f )
	{
		ResetStatus();

		multiplier = Math.Clamp( multiplier, 0.1f, 1f );
		var volume = MathX.Remap( multiplier, 0.1f, 1f, 0.3f, 1f );
		var pitch = MathX.Remap( multiplier, 0.1f, 1f, 1.4f, 0.5f );

		// PlaySound( "sounds/crash/crash.sound" )
		// 	.SetVolume( multiplier );

		StunLeft = StunDuration * multiplier;
	}

	public float TripDuration { get; set; } = 1.5f;
	public bool IsTripping => !TripLeft;
	[Sync( Query = true )] public TimeUntil TripLeft { get; set; }

	public void Trip()
	{
		ResetStatus();
		//PlaySound( "sounds/pipe.sound" )
		//	.SetVolume( multiplier );

		TripLeft = TripDuration;
	}

	public float SlipDuration { get; set; } = 2f;
	public bool IsSlipping => !SlipLeft;
	[Sync( Query = true )] public TimeUntil SlipLeft { get; set; }

	public void Slip()
	{
		ResetStatus();
		//PlaySound( "sounds/pipe.sound" )
		//	.SetVolume( multiplier );

		SlipLeft = SlipDuration;

		Velocity = (Velocity.WithZ( 0 ).Normal * RunSpeed).WithZ( Velocity.z );
	}

	public void ResetStatus()
	{
		StunLeft = -1f;
		TripLeft = -1f;
		SlipLeft = -1f;
	}

	private void CalculateTrip()
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
			var capsuleTrace = Scene.Trace.Capsule( lowerCapsule, Transform.Position, Transform.Position + Velocity.WithZ( 0 ) * Time.Delta );
			capsuleTrace = capsuleTrace
				.WithTag( "loot" )
				.IgnoreGameObject( GameObject );
			var tr = capsuleTrace.Run();

			if ( tr.Hit )
				Trip();
		}
	}

	private void CalculateSlip()
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
			var capsuleTrace = Scene.Trace.Capsule( lowerCapsule, Transform.Position, Transform.Position + Velocity.WithZ( 0 ) * Time.Delta );
			capsuleTrace = capsuleTrace
				.WithTag( "slip" )
				.IgnoreGameObject( GameObject );
			var tr = capsuleTrace.Run();

			if ( tr.Hit )
				Slip();
		}
	}

	private void CalculateStun()
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
			var capsuleTrace = Scene.Trace.Capsule( higherCapsule, Transform.Position, Transform.Position + Velocity.WithZ( 0 ) * Time.Delta );
			capsuleTrace = capsuleTrace
				.WithoutTags( ITH.Tag.NoCollide, ITH.Tag.Loot, ITH.Tag.Doob )
				.IgnoreGameObject( GameObject );
			var tr = capsuleTrace.Run();

			if ( tr.Hit )
			{
				var dotProduct = Math.Abs( Vector3.Dot( Velocity.WithZ( 0 ).Normal, tr.Normal ) );
				var wallVelocity = Velocity.WithZ( 0 ) * dotProduct;
				var difference = MathX.Remap( wallVelocity.Length, WalkSpeed, RunSpeed );

				if ( wallVelocity.Length > WalkSpeed )
				{
					// TODO: Stun other player

					// if ( tr.Entity is Player other )
					// {
					// 	difference /= 2f;

					// 	if ( Networking.IsHost )
					// 	{
					// 		var random = MansionGame.Random.Float( 0f, 1f );
					// 		if ( random < DropChance )
					// 			other.ThrowRandomLoot();

					// 		other.Stun( difference );

					// 		other.Velocity -= tr.Normal * (CollisionRadius + StunBounceVelocity - Velocity.WithZ( 0 ).Length);
					// 		other.Rotation = Rotation.LookAt( tr.Normal, Vector3.Up );
					// 	}
					// }

					Stun( difference );

					// if ( Networking.IsHost )
					// {
					// 	Particles.Create( "particles/smoke/smoke_impact.vpcf", Position + Rotation.Forward * CollisionRadius + Rotation.Up * CollisionHeight / 2f );
					// 	var impact = Particles.Create( "particles/impact/impact.vpcf", Position + Rotation.Forward * CollisionRadius + Rotation.Up * CollisionHeight / 2f );

					// 	impact.SetForward( 1, tr.Normal );
					// }

					Velocity += tr.Normal * (CollisionRadius + StunBounceVelocity);
					Transform.Rotation = Rotation.LookAt( -tr.Normal, Vector3.Up );

					// Let's randomly throw out an item when we crash.
					// if ( Networking.IsHost )
					// {
					// 	var random = MansionGame.Random.Float( 0f, 1f );
					// 	if ( random < DropChance )
					// 		ThrowRandomLoot();
					// }
				}
			}
		}
	}

}
