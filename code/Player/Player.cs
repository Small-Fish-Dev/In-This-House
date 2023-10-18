namespace BrickJam;

partial class Player : AnimatedEntity
{
	[BindComponent] public ContainerComponent Inventory { get; } 

	[Net, Change] public int Money { get; private set; }

	public float CollisionRadius => 12f;
	public float CollisionHeight => IsCrouching ?  36f : 72f;
	public Capsule CollisionCapsule => new Capsule( Vector3.Up * CollisionRadius, Vector3.Up * (CollisionHeight - CollisionRadius), CollisionRadius );
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/robber/robber.vmdl" );
		RebuildCollisions();

		Tags.Add( "player" );

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = false; // For firstperson legs!!

		// Remember to create the container component!!!
		Components.GetOrCreate<ContainerComponent>();

		// Mask
		var mask = new ModelEntity();
		mask.SetModel( "models/robber/mask.vmdl" );
		mask.SetParent( this, true );
		mask.Transmit = TransmitType.Always;
		mask.EnableHideInFirstPerson = true;
	}

	public void RebuildCollisions() => SetupPhysicsFromOrientedCapsule( PhysicsMotionType.Keyframed, CollisionCapsule );


	[ClientInput] public Vector3 InputDirection { get; protected set; }
	[ClientInput] public Angles InputAngles { get; protected set; }
	[ClientInput] public bool IsRunning { get; protected set; }
	[ClientInput]
	public bool IsCrouching
	{
		get => crouching;
		protected set
		{
			if ( value != crouching )
			{
				crouching = value;
				RebuildCollisions();
			}
		}
	}


	public bool crouching = false;

	public Rotation InputRotation => InputAngles.ToRotation();
	public Vector3 EyePosition => Position + Vector3.Up * ( IsCrouching ? 28f : 64f );

	public override void BuildInput()
	{
		if ( CanUse )
		{
			InputDirection = Input.AnalogMove;

			InputAngles += Input.AnalogLook;
			InputAngles = InputAngles.WithPitch( Math.Clamp( InputAngles.pitch, -89.9f, 89.9f ) );
		}

		if ( !MovementLocked )
		{
			IsRunning = Input.Down( "run" );
			IsCrouching = Input.Down( "crouch" );
		}
		else
		{
			IsRunning = false;
			IsCrouching = false;
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

		HandleBodyiew();

		if ( !MovementLocked )
		{
			Camera.Position = GetAttachment( "eyes" )?.Position ?? EyePosition;
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

		Camera.ZNear = 2;
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( 60f );
		
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
					v.Value( "croushing?", IsCrouching );
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

		var remapped = MathX.Remap( Velocity.Length, 0, 150, 0.5f, 1f );
		var animationHelper = new CitizenAnimationHelper( this );
		animationHelper.WithVelocity( Velocity );
		animationHelper.WithLookAt( Position + InputRotation.Forward * 10f, 0f, remapped, 0.8f );
		animationHelper.DuckLevel = IsCrouching ? 2f : 0f;

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
