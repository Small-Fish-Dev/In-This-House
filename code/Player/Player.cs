using Sandbox;
using System;
using System.Linq;

namespace BrickJam;

partial class Player : AnimatedEntity
{
	[Net, Change] public int Money { get; private set; }

	public BBox CollisionBox => new( new Vector3( -12f, -12f, 0f ), new Vector3( 12f, 12f, 72f ) );
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen/citizen.vmdl" );
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, CollisionBox.Mins, CollisionBox.Maxs );

		Tags.Add( "player" );

		EnableAllCollisions = true;
		EnableDrawing = true;
	}


	[ClientInput] public Vector3 InputDirection { get; protected set; }
	[ClientInput] public Angles InputAngles { get; protected set; }
	[ClientInput] public bool IsRunning { get; protected set; } = false;
	public Rotation InputRotation => InputAngles.ToRotation();
	public Vector3 EyePosition => Position + Vector3.Up * 64f;

	public override void BuildInput()
	{
		InputDirection = Input.AnalogMove;

		InputAngles += Input.AnalogLook;
		InputAngles = InputAngles.WithPitch( Math.Clamp( InputAngles.pitch, -89.9f, 89.9f ) );

		IsRunning = Input.Down( "run" );
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

		Camera.Position = EyePosition;
		Camera.Rotation = InputRotation;

		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );
		Camera.FirstPersonViewer = this; // Doesn't work?
		EnableDrawing = false; // Let's use this for now
		
		BugBug.Here( v =>
		{
			v.Group( "local player", () =>
			{
				v.Value( "pos", Game.LocalPawn.Position );
				v.Value( "ang", Game.LocalPawn.Rotation.Angles() );
				v.Value( "vel", Game.LocalPawn.Velocity );
			} );
		} );
	}

	protected void SimulateAnimations()
	{
		Rotation = Rotation.FromYaw( InputAngles.yaw );

		var animationHelper = new CitizenAnimationHelper( this );
		animationHelper.WithVelocity( Velocity );

		animationHelper.IsGrounded = GroundEntity != null;
	}

	public void Respawn()
	{
		var spawnpoints = Entity.All.OfType<PlayerSpawn>();

		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		if ( randomSpawnPoint != null )
		{
			var tx = randomSpawnPoint.Transform;
			tx.Position = tx.Position + Vector3.Up * 50.0f; // raise it up
			Transform = tx;
		}
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
