namespace BrickJam;

partial class Player : AnimatedEntity
{
	[BindComponent] public ContainerComponent Inventory { get; } 

	[Net, Change] public int Money { get; private set; }

	public virtual float CollisionRadius { get; set; } = 12f;
	public virtual float CollisionHeight { get; set; } = 72f;
	public Capsule CollisionCapsule => new Capsule( Vector3.Up * CollisionRadius, Vector3.Up * (CollisionHeight - CollisionRadius), CollisionRadius );
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/robber/robber.vmdl" );
		SetupPhysicsFromOrientedCapsule( PhysicsMotionType.Keyframed, CollisionCapsule );

		Tags.Add( "player" );

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;

		// Remember to create the container component!!!
		Components.GetOrCreate<ContainerComponent>();
	}


	[ClientInput] public Vector3 InputDirection { get; protected set; }
	[ClientInput] public Angles InputAngles { get; protected set; }
	public Rotation InputRotation => InputAngles.ToRotation();
	public Vector3 EyePosition => Position + Vector3.Up * 64f;

	public override void BuildInput()
	{
		if ( CanUse )
		{
			InputDirection = Input.AnalogMove;

			InputAngles += Input.AnalogLook;
			InputAngles = InputAngles.WithPitch( Math.Clamp( InputAngles.pitch, -89.9f, 89.9f ) );
		}
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		SimulateController();
		SimulateAnimations();
		SimulateUse();
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		if ( !IsSlipping && !IsTripping )
		{
			Camera.Position = EyePosition;
			Camera.Rotation = InputRotation;
			Camera.FirstPersonViewer = this;
		}
		else
		{
			var newPos = Position - Velocity.WithZ( 0 ).Normal * 30f + Vector3.Up * 50f;
			Camera.Position = Vector3.Lerp( Camera.Position, newPos, Time.Delta * 10 );
			Camera.Rotation = Rotation.LookAt( Position + Vector3.Up * 16f - Camera.Position );
			Camera.FirstPersonViewer = null;
		}

		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );
		
		BugBug.Here( v =>
		{
			v.Group( "local player", () =>
			{
				v.Value( "pos", Game.LocalPawn.Position );
				v.Value( "ang", Game.LocalPawn.Rotation.Angles() );
				v.Value( "vel", Game.LocalPawn.Velocity );
				v.Value( "money", Money );
				
				v.Group( "movement", () =>
				{
					v.Value( "running?", IsRunning );
					v.Value( "wish speed", WishSpeed );
				} );
			} );
		} );
	}

	protected void SimulateAnimations()
	{
		if ( MovementLocked )
			Rotation = Rotation.LookAt( Velocity.WithZ( 0 ), Vector3.Up );
		else
			Rotation = Rotation.FromYaw( InputAngles.yaw );

		var animationHelper = new CitizenAnimationHelper( this );
		animationHelper.WithVelocity( Velocity );
		animationHelper.WithLookAt( Position + InputRotation.Forward * 10f );

		animationHelper.IsGrounded = GroundEntity != null;
		SetAnimParameter( "special_movement_states", IsStunned ? 1 : ( IsTripping ? 2 : ( IsSlipping ? 3 : 0 ) ) );
		SetAnimParameter( "speed_scale", Velocity.WithZ(0).Length / 150f );
	}

	public void Respawn()
	{
		var t = MansionGame.Instance.GetSpawnPoint();
		t.Position += Vector3.Up * 50.0f;
		Transform = t;
	}

	// Client callback for UI purposes
	public void OnMoneyChanged( int oldValue, int newValue )
	{
		Event.Run( "MoneyChanged", this, oldValue, newValue );
	}

	// Set player's money
	public void SetMoney( int value )
	{
		Event.Run( "MoneyChanged", this, Money, value );
		Money = value;
	}

	// Add to the players money
	public void AddMoney( int value ) => SetMoney( Money + value );

	// Remove to the players money (Clamps to 0)
	public void RemoveMoney( int value ) => SetMoney( Math.Max( Money + value, 0 ) );
}
