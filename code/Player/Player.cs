using BrickJam.UI;

namespace BrickJam;

partial class Player : AnimatedEntity, IPushable
{
	[BindComponent] public ContainerComponent Inventory { get; } 

	[Net, Change] public int Money { get; private set; }
	[Net] public bool IsAlive { get; private set; } = false;

	public float CollisionRadius => IsCrouching ? 22f : 12f;
	public float CollisionHeight => IsCrouching ?  36f : 72f;
	public Capsule CollisionCapsule => new Capsule( Vector3.Up * CollisionRadius, Vector3.Up * (CollisionHeight - CollisionRadius), CollisionRadius );
	[Net] public Doob Doob { get; set; } = null;

	[Net] public NPC CameraTarget { get; set; } = null;
	public float PushForce { get; set; } = 2500f;

	[Net] protected ModelEntity Mask { get; set; }

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		// Load save
		var save = PlayerSave.LoadStored();
		if ( save != null )
		{
			Log.Info( "Sending save to the server" );
			SendSaveToServer( save.Value );
		}
	}
	
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/robber/robber.vmdl" );
		RebuildCollisions();

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = false; // For firstperson legs!!

		// Remember to create the container component!!!
		Components.GetOrCreate<ContainerComponent>();

		// Mask
		Mask = new ModelEntity();
		Mask.SetModel( "models/robber/mask.vmdl" );
		Mask.SetParent( this, true );
		Mask.Transmit = TransmitType.Always;
		Mask.EnableHideInFirstPerson = true;
	}

	public void RebuildCollisions()
	{
		SetupPhysicsFromOrientedCapsule( PhysicsMotionType.Keyframed, CollisionCapsule );
		EnableTouch = true;
		Tags.Add( "player" );
	}


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
		if ( !CommandsLocked )
		{
			if ( !Lockpicker.Active )
			{
				InputDirection = Input.AnalogMove;

				InputAngles += Input.AnalogLook;
				InputAngles = InputAngles.WithPitch( Math.Clamp( InputAngles.pitch, -80f, 80f ) );
			}
			else
			{
				InputDirection = 0;
			}
		}

		if ( !MovementLocked && !Lockpicker.Active )
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

		HandleBodyview();

		if ( CameraTarget == null )
		{
			if ( !MovementLocked )
			{
				Camera.Rotation = InputRotation;
				Camera.Position = GetAttachment( "eyes" )?.Position - InputRotation.Up * 2f - Rotation.Forward * 3f ?? EyePosition;
				Camera.FirstPersonViewer = this;
			}
			else
			{
				var newPos = Position - Velocity.WithZ( 0 ).Normal * 30f + Vector3.Up * 50f;
				Camera.Rotation = Rotation.LookAt( Position + Vector3.Up * 16f - Camera.Position );
				Camera.Position = Vector3.Lerp( Camera.Position, newPos, Time.Delta * 10 );
				Camera.FirstPersonViewer = null;
			}
		}
		else
		{
			var headPos = CameraTarget.GetBoneTransform( CameraTarget.GetBoneIndex( "head" ) ).Position;
			var rotation = Rotation.LookAt( headPos - Camera.Position );
			Camera.Position = GetAttachment( "eyes" )?.Position - rotation.Up * 2f - rotation.Forward * 3f ?? EyePosition;
			Camera.Rotation = rotation;
		}
		

		Camera.ZNear = 2;
		Camera.ZFar = 4096;
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
		if ( CameraTarget == null )
		{
			if ( MovementLocked )
				Rotation = Rotation.LookAt( Velocity.WithZ( 0 ), Vector3.Up );
			else
			{
				if ( !IsStunned )
					Rotation = Rotation.FromYaw( InputAngles.yaw );
			}
		}
		else
		{
			var rotation = Rotation.LookAt( CameraTarget.Position - Position, Vector3.Up );
			Rotation = Rotation.Lerp( Rotation, rotation, Time.Delta * 5f );
		}

		var remapped = MathX.Remap( Velocity.Length, 0, 150, 0.5f, 1f );
		var animationHelper = new CitizenAnimationHelper( this );
		animationHelper.WithVelocity( Velocity );
		animationHelper.WithLookAt( Position + InputRotation.Forward * 10f, 0f, remapped, 0.8f );
		animationHelper.DuckLevel = IsCrouching ? 2f : 0f;

		animationHelper.IsGrounded = GroundEntity != null;
		SetAnimParameter( "special_movement_states", IsStunned ? 1 : ( IsTripping ? 2 : ( IsSlipping ? 3 : 0 ) ) );
		SetAnimParameter( "speed_scale", Velocity.WithZ(0).Length / 150f );
	}

	internal List<AnimatedEntity> toPush = new();

	public override void StartTouch( Entity other )
	{
		base.Touch( other );

		if ( !Game.IsServer ) return;
		if ( other is IPushable toucher )
			toPush.Add( toucher as AnimatedEntity );
	}

	public override void EndTouch( Entity other )
	{
		base.Touch( other );

		if ( !Game.IsServer ) return;

		if ( other is IPushable toucher && toPush.Contains( toucher as AnimatedEntity ) )
			toPush.Remove( toucher as AnimatedEntity );
	}

	public override void OnAnimEventFootstep( Vector3 position, int foot, float volume )
	{
		base.OnAnimEventFootstep( position, foot, volume );

		if ( GroundEntity != null )
		{
			string levelSound = MansionGame.Instance.CurrentLevel.Type switch
			{
				LevelType.Mansion => "sounds/footsteps/footstep-wood.sound",
				LevelType.Shop => "sounds/footsteps/footstep-wood.sound",
				_ => "sounds/footsteps/footstep-concrete.sound",
			};

			var footstep = Sound.FromWorld( levelSound, position );

			var newVolume = MathX.Remap( Velocity.Length, WalkSpeed, RunSpeed, 1f, 2f );
			footstep.SetVolume( newVolume );
		}	
	}

	protected void PlayJumpSound()
	{
		string levelSound = MansionGame.Instance.CurrentLevel.Type switch
		{
			LevelType.Mansion => "sounds/footsteps/footstep-wood-jump.sound",
			LevelType.Shop => "sounds/footsteps/footstep-wood.sound",
			_ => "sounds/footsteps/footstep-concrete-jump.sound",
		};

		var footstep = PlaySound( levelSound );
		var newVolume = MathX.Remap( Velocity.Length, 0, RunSpeed, 0.6f, 1f );
		footstep.SetVolume( newVolume );
	}
	protected void PlayLandSound()
	{
		string levelSound = MansionGame.Instance.CurrentLevel.Type switch
		{
			LevelType.Mansion => "sounds/footsteps/footstep-wood-land.sound",
			LevelType.Shop => "sounds/footsteps/footstep-wood.sound",
			_ => "sounds/footsteps/footstep-concrete-land.sound",
		};

		var footstep = PlaySound( levelSound );
		var newVolume = MathX.Remap( Velocity.WithZ( 0 ).Length, 0, 300, 0.5f, 2f, false );
		footstep.SetVolume( newVolume );
	}

	public void Respawn()
	{
		var t = MansionGame.Instance.GetSpawnPoint();
		t.Position += Vector3.Up * 50.0f;
		Transform = t;

		IsAlive = true;
		EnableDrawing = true;
		EnableAllCollisions = true;
		Blocked = false;
		
		SetMaskTint(Client.GetColor());
	}

	[ClientRpc]
	protected void SetMaskTint(Color color)
	{
		// color.Desaturate( 0.1f ) if needed
		Mask.SceneObject.Attributes.Set( "maskTint", color );
	}

	public void Kill()
	{
		if ( !IsAlive ) return;

		IsAlive = false;
		EnableDrawing = false;
		EnableAllCollisions = false;
		Blocked = false;

		/*if ( Components.TryGet<ContainerComponent>( out ContainerComponent inventory ) )
			inventory.Clear();

		ClearInventory( To.Single( Client ) );*/ // Comment out in case of disaster

		Particles.Create( "particles/blood/blood_explosion.vpcf", Position );
		Sound.FromWorld( "sounds/death.sound", Position );

		if ( Entity.All.OfType<Player>().Where( x => x.IsAlive ).Count() > 0 )
		{
			// Log.Error( "TODO: FUCKIG INPUTROTATION IS NOT NETWORKED TO EVERYONE FUUUUUUUUCKK" );
			var spectator = new Spectator { Position = EyePosition, Rotation = Rotation, Body = this }; // TODO: Rotation->InputRotation
			Client.Pawn = spectator;
		}
	}

	[ClientRpc]
	public void ClearInventory()
	{
		if ( Components.TryGet<ContainerComponent>( out ContainerComponent inventory ) )
			inventory.Clear();
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
		Client.SetInt( "money", Money );
	}

	// Add to the players money
	public void AddMoney( int value ) => SetMoney( Money + value );

	// Remove to the players money (Clamps to 0)
	public void RemoveMoney( int value ) => SetMoney( Math.Max( Money + value, 0 ) );

	[ClientRpc]
	public static void _sendMessage( int indent, string message )
	{
		if ( Entity.FindByIndex( indent ) is not Player player )
			return;

		Speechbubble.Create( message, player );
	}

	[ClientRpc]
	public static void _addEventlog( string text, float time )
	{
		Eventlog.Instance?.Append( text, time );
	}

	[ConCmd.Server]
	public static void SendMessage( string message )
	{
		if ( ConsoleSystem.Caller.Pawn is not Player pawn )
			return;

		if ( string.IsNullOrEmpty( message ) || message.Length > 200 )
			return;

		_sendMessage( pawn.NetworkIdent, message );
	}

	[ConCmd.Server]
	public static void GiveMoney( int amount )
	{
		if ( ConsoleSystem.Caller.Pawn is not Player pawn )
			return;

		pawn.AddMoney( amount );
	}
}
