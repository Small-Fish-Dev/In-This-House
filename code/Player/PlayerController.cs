namespace ITH;

public sealed partial class PlayerController : Component
{
	[Property] public SkinnedModelRenderer Model { get; set; }
	[Property] public GameObject Eyes { get; set; }
	[Property] public GameObject HeadBone { get; set; }
	[Property] public SoundPointComponent skiddingSound { get; private set; }
	[Property] public GameObject skiddingParticlesPrefab { get; private set; }
	[Property] private CameraComponent _camera;

	private Player _player; // cyclical references, cope about it.
	private CharacterController _controller;
	private Sandbox.Citizen.CitizenAnimationHelper _animator;

	[Sync] public float CrouchSpeed { get; set; } = 80f;
	[Sync] public float WalkSpeed { get; set; } = 200f;
	[Sync] public float RunSpeed { get; set; } = 350f;
	[Sync] public float JumpHeight { get; set; } = 250f;
	[Sync] public float Acceleration { get; set; } = 1000f;
	[Sync] public float Deceleration { get; set; } = 400f;
	// public bool HasFrictionUpgrade => HasUpgrade( "Work Shoes" );
	public bool HasFrictionUpgrade => false;

	public float StunSpeed => (float)(WalkSpeed + (RunSpeed - WalkSpeed) * Math.Sin( 45f.DegreeToRadian() ));
	public float WishSpeed => InputDirection.IsNearlyZero() ? 0 : (IsCrouching ? CrouchSpeed : (IsRunning ? RunSpeed : WalkSpeed));
	public Vector3 WishVelocity => (InputDirection.IsNearlyZero() || IsStunned)
		? Vector3.Zero
		: InputDirection * Rotation.FromYaw( InputAngles.yaw ) * WishSpeed;
	public bool IsAboveWalkingSpeed => _controller.Velocity.WithZ( 0 ).Length >= MathX.Lerp( WalkSpeed, RunSpeed, 0.5f );

	public float StepSize => 16f;
	public float WalkAngle => 70f;
	public float StunBounceVelocity => 550f;
	public float CollisionRadius => IsCrouching ? 22f : 12f;
	public float CollisionHeight => IsCrouching ? 36f : 72f;
	public Capsule CollisionCapsule => new Capsule( Vector3.Up * CollisionRadius, Vector3.Up * (CollisionHeight - CollisionRadius), CollisionRadius );
	public Vector3 EyePosition { get; private set; }
	public Rotation EyeRotation { get; private set; }

	static string[] ignoreTags = new[] { "player", "npc", "nocollide", "loot" };
	float baseSkiddingVolume = 0.3f;
	float skiddingVolume;
	[Property] ParticleEffect skidParticle;
	TimeSince lastAcceleration;

	protected override void OnStart()
	{
		_player = Components.Get<Player>();
		_controller = Components.Get<CharacterController>();
		_animator = Components.Get<Sandbox.Citizen.CitizenAnimationHelper>();
		_camera = Scene.Camera;

		if ( !IsProxy )
		{
			Local.Pawn = GameObject;
		}
	}

	protected override void OnFixedUpdate()
	{
		UpdateMovement();
		UpdateUse();
	}

	protected override void OnUpdate()
	{
		if ( !IsProxy )
		{
			BuildInput();
		}

		UpdateAnimation();
	}

	protected override void OnPreRender()
	{
		if ( IsProxy )
			return;

		var lookDir = InputAngles.ToRotation();
		var eyesAtx = Model.GetAttachment( "eyes", true );
		if ( !eyesAtx.HasValue )
		{
			Log.Error( "Eyes need an attachment fuck" );
			return;
		}

		// TODO: (ARTIST) Put the head in a bodygroup so we can just turn it off.
		// Model.SetBodyGroup( "head", 0 );
		Model.SceneModel.GetBoneLocalTransform( "head" );

		Eyes.Transform.Position = eyesAtx.Value.Position;
		EyePosition = eyesAtx.Value.Position + eyesAtx.Value.Left * 3;
		EyeRotation = lookDir;

		_camera.Transform.Position = EyePosition;
		_camera.Transform.Rotation = EyeRotation;
		_camera.FieldOfView = Screen.CreateVerticalFieldOfView( Preferences.FieldOfView );
	}

	private void UpdateMovement()
	{
		if ( !MovementLocked )
		{
			if ( WishVelocity.Length >= Velocity.WithZ( 0 ).Length ) // Accelerating
			{
				var momentumCoefficent = Velocity.WithZ( 0 ).Length / WalkSpeed * 50f; // Slower you move, less momentum you have, easier to start
				Velocity += WishVelocity.WithZ( 0 ).Normal * (Acceleration - momentumCoefficent) * (HasFrictionUpgrade ? 3f : 1f) * Time.Delta;
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
			skiddingSound.Enabled = true;
			skiddingSound.StartSound();

			// skidParticle.Enabled = true;
			skidParticle.Emit( 1 );
		}
		else
		{
			skiddingSound.Enabled = false;
			skiddingSound.StopSound();

			// skidParticle.Enabled = false;
		}

		skiddingSound.Volume = skiddingVolume;

		Velocity += Scene.PhysicsWorld.Gravity * Time.Delta;

		_controller.Velocity = Velocity;
		_controller.Move();

		if ( IsAboveWalkingSpeed )
		{
			CalculateStun();
			CalculateTrip();
		}
	}

	private void UpdateAnimation()
	{
		var body = _animator.Target.GameObject;
		var rotateDifference = 0f;
		if ( body is not null )
		{
			var targetAngle = new Angles( 0, InputAngles.yaw, 0 ).ToRotation();
			rotateDifference = body.Transform.Rotation.Distance( targetAngle );
			body.Transform.Rotation = Rotation.Lerp( body.Transform.Rotation, targetAngle, Time.Delta * 22.0f );
		}

		_animator.IsGrounded = _controller.IsOnGround;
		_animator.FootShuffle = rotateDifference;
		_animator.WithWishVelocity( WishVelocity );
		_animator.WithVelocity( Velocity );
		_animator.WithLook( InputAngles.Forward, 0.5f, 0.25f, 0.1f );

		Model.Set( "special_movement_states", IsStunned ? 1 : (IsTripping ? 2 : (IsSlipping ? 3 : 0)) );
		Model.Set( "speed_scale", Velocity.WithZ( 0 ).Length / 150f );
	}

	[Authority]
	public void Respawn( Vector3 position )
	{
		Transform.Position = position;
	}
}
