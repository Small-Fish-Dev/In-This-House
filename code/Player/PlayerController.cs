namespace ITH;

public sealed class PlayerController : Component
{
	[Property] public SkinnedModelRenderer Model { get; set; }
	[Property] public GameObject Eyes { get; set; }

	[Net] public float CrouchSpeed { get; set; } = 80f;
	[Net] public float WalkSpeed { get; set; } = 200f;
	[Net] public float RunSpeed { get; set; } = 350f;
	[Net] public float JumpHeight { get; set; } = 250f;
	[Net] public float Acceleration { get; set; } = 1000f;
	[Net] public float Deceleration { get; set; } = 400f;
	// public bool HasFrictionUpgrade => HasUpgrade( "Work Shoes" );
	public bool HasFrictionUpgrade => false;

	public bool MovementLocked = false;
	public float StunSpeed => (float)(WalkSpeed + (RunSpeed - WalkSpeed) * Math.Sin( 45f.DegreeToRadian() ));
	public float WishSpeed => InputDirection.IsNearlyZero() ? 0 : (IsCrouching ? CrouchSpeed : (IsRunning ? RunSpeed : WalkSpeed));
	public Vector3 WishVelocity => (InputDirection.IsNearlyZero() || IsStunned)
		? Vector3.Zero
		: InputDirection * Rotation.FromYaw( InputAngles.yaw ) * WishSpeed;
	public bool IsAboveWalkingSpeed => _controller.Velocity.WithZ( 0 ).Length >= MathX.Lerp( WalkSpeed, RunSpeed, 0.5f );

	public float StepSize => 16f;
	public float WalkAngle => 70f;
	public float StunBounceVelocity => 550f;

	static string[] ignoreTags = new[] { "player", "npc", "nocollide", "loot" };
	float baseSkiddingVolume = 0.3f;
	float skiddingVolume;
	TimeSince lastAcceleration;

	private Vector3 InputDirection;
	private Angles InputAngles;
	private bool IsCrouching = false;
	private bool IsStunned = false;
	private bool IsRunning = false;
	public Vector3 Velocity { get; private set; }

	private CharacterController _controller;
	private Sandbox.Citizen.CitizenAnimationHelper _animator;
	private CameraComponent _camera;

	protected override void OnStart()
	{
		if ( !IsProxy )
		{
			if ( !Game.IsEditor )
				Model.RenderType = ModelRenderer.ShadowRenderType.ShadowsOnly;
		}

		_controller = Components.Get<CharacterController>();
		_animator = Components.Get<Sandbox.Citizen.CitizenAnimationHelper>();
		_camera = Scene.Camera;
	}

	protected override void OnUpdate()
	{
		if ( !IsProxy )
		{
			InputDirection = Input.AnalogMove;
			InputAngles += Input.AnalogLook * Time.Delta * 350;
			InputAngles.pitch = MathX.Clamp( InputAngles.pitch, -89.0f, 89.0f );

			UpdateMovement();
			UpdateAnimation();

			var lookDir = InputAngles.ToRotation();
			_camera.Transform.Position = Eyes.Transform.Position;
			_camera.Transform.Rotation = lookDir;
			_camera.FieldOfView = 90;
		}
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

		Velocity += Scene.PhysicsWorld.Gravity * Time.Delta;
		_controller.Velocity = Velocity;
		_controller.Move();
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
	}
}
