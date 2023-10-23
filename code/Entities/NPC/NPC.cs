using GridAStar;
using Sandbox;

namespace BrickJam;

public partial class NPC : AnimatedEntity, IPushable
{
	public Level Level { get; private set; }
	public virtual string ModelPath { get; set; } = "models/citizen/citizen.vmdl";
	public virtual float CollisionRadius { get; set; } = 12f;
	public virtual float CollisionHeight { get; set; } = 72f;
	public Capsule CollisionCapsule => new Capsule( Vector3.Up * CollisionRadius, Vector3.Up * (CollisionHeight - CollisionRadius), CollisionRadius );
	public virtual float MaxVisionRange { get; set; } = 1024f;
	public virtual float MaxVisionAngle { get; set; } = 120f; // Degrees
	public virtual float MaxVisionAngleWhenChasing { get; set; } = 180f;
	public virtual float MaxVisionRangeWhenChasing { get; set; } = 2048f;
	public virtual float MaxRememberTime { get; set; } = 1f; // How long will it keep tracking your position when you're out of sight (This lets them get into rooms or corridors you hide in)
	public virtual float AttackAnimationDuration { get; set; } = 1.5f;
	public virtual float KillAfterAttackTime { get; set; } = 1f;
	public virtual float SetDistanceWhenAttacking { get; set; } = 40f;
	public virtual float KillRange { get; set; } = 60f;
	public Dictionary<AnimatedEntity, TimeSince> InVision { get; private set; } = new();
	public AnimatedEntity Target { get; set; } = null;
	public AnimatedEntity LastTarget { get; set; } = null;
	public AnimatedEntity CurrentlyMurdering { get; set; } = null;
	internal TimeUntil nextIdle { get; set; } = 0f;
	public float PushForce { get; set; } = 3000f;
	public virtual string IdleSound => "sounds/nyobo/nyobo_laugh.sound";
	public virtual float IdleVolume => 2f;
	public virtual string AttackSound => "sounds/nyobo/nyobo_attack.sound";
	public virtual float AttackVolume => 2f;

	public NPC() { }
	public NPC( Level level ) : base()
	{
		Level = level;
	}

	public override void Spawn()
	{
		base.Spawn();

		SetModel( ModelPath );
		SetupPhysicsFromOrientedCapsule( PhysicsMotionType.Keyframed, CollisionCapsule );
		Tags.Add( "npc" );
	}

	[GameEvent.Tick.Server]
	public virtual void Think()
	{
		FindTargets();

		ComputeIdleAndSeek();
		ComputeOpenDoors();
		ComputeNavigation();
		ComputeMotion();
		AssignNearbyTags();
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

	internal TimeUntil nextIdleSound = 2f;
	public virtual void ComputeIdleAndSeek()
	{
		if ( InVision.Count > 0 )
		{
			Target = InVision.OrderBy( x => x.Key.Position.Distance( Position ) )
				.FirstOrDefault().Key;
			if ( Target is Player player )
				if ( player.Doob != null )
					Target = player.Doob;

			LastTarget = Target;
		}
		else
			Target = null;

		if ( Target == null )
		{
			if ( !IsFollowingPath )
			{
				if ( nextIdle )
				{
					var isLongIdle = MansionGame.Random.Float() <= 0.2f;

					Cell chosenCell = null;
					var tried = 0;
					while ( chosenCell != null && isLongIdle ? chosenCell.Position.Distance( Position ) < 1000f : chosenCell?.Position.Distance( Position ) > 200f || chosenCell == null )
					{
						chosenCell = MansionGame.Random.FromList( Level.Grid?.AllCells.ToList() ) ?? null;
						tried++;

						if ( tried >= 20 )
							break;
					}

					if ( chosenCell != null )
						NavigateTo( chosenCell );

					nextIdle = MansionGame.Random.Float( 1f, 2f );

					LastTarget = null;
				}
			}
		}

		if ( Target != null ) // Kill player is in range
			if ( Target.Position.Distance( Position ) <= KillRange )
			{
				if ( Target is Player player )
					CatchPlayer( player );
				if ( Target is Doob doob )
					CatchDoob( doob );
			}

		if ( nextIdleSound )
		{
			var idle = PlaySound( IdleSound );
			idle.SetVolume( IdleVolume );
			nextIdleSound = Game.Random.Float( 4f, 8f );
		}
	}

	public virtual void ComputeOpenDoors()
	{
		var allDoors = Entity.All.OfType<Door>();
		var nearbyDoors = allDoors.Where( x => x.Position.Distance( Position ) <= 60f ).ToList();

		foreach ( var door in nearbyDoors )
		{
			if ( door.State == DoorState.Closed || door.State == DoorState.Closing )
			{
				door.Open( this );

				GameTask.RunInThreadAsync( async () =>
				{
					await GameTask.Delay( 1500 );
					door.Close();
				} );
			}
		}
	}

	public async virtual void CatchPlayer( Player player )
	{
		if ( CurrentlyMurdering != null || CurrentlyMurdering.IsValid() ) return;

		SetAnimParameter( "attack", true );
		Sound.FromWorld( "sounds/screams/scream.sound", player.Position );

		var attack = PlaySound( AttackSound );
		attack.SetVolume( AttackVolume );

		var currentDirection = (player.Position - Position).Normal;
		player.Position = Position + currentDirection * SetDistanceWhenAttacking;
		player.CameraTarget = this;
		player.Blocked = true;

		CurrentlyMurdering = player;
		Blocked = true;

		await GameTask.DelaySeconds( KillAfterAttackTime );

		player.Kill();

		await GameTask.DelaySeconds( AttackAnimationDuration );

		player.CameraTarget = null;
		player.Blocked = false;

		CurrentlyMurdering = null;
		Blocked = false;
	}

	public async virtual void CatchDoob( Doob doob )
	{
		if ( CurrentlyMurdering != null || CurrentlyMurdering.IsValid() ) return;

		SetAnimParameter( "attack", true );

		var attack = PlaySound( AttackSound );
		attack.SetVolume( AttackVolume );

		var currentDirection = (doob.Position - Position).Normal;
		doob.Position = Position + currentDirection * SetDistanceWhenAttacking;
		doob.Blocked = true;

		CurrentlyMurdering = doob;
		Blocked = true;

		await GameTask.DelaySeconds( KillAfterAttackTime );

		doob.Kill();

		await GameTask.DelaySeconds( AttackAnimationDuration );

		doob.Blocked = false;

		CurrentlyMurdering = null;
		Blocked = false;
	}

	[GameEvent.Tick]
	public virtual void ComputeAnimations()
	{
		SetAnimParameter( "move_x", MathX.Remap( Velocity.WithZ(0).Length, 0f, RunSpeed, 0, 3 ) );
	}

	public virtual void FindTargets()
	{
		// Remove all the invalid players
		foreach (var player in InVision.Where( x => !x.Key.IsValid()).ToList()) // Make a copy of InVision
			InVision.Remove( player.Key );

		// Remove all dead players
		foreach ( var player in InVision.Where( x => x.Key is Player player && !player.IsAlive ).ToList() ) // Make a copy of InVision
			InVision.Remove( player.Key );

		foreach ( var player in Entity.All.OfType<Player>().Where( x => x.IsAlive ).ToList() )
		{
			if ( IsInVision( player ) )
			{
				if ( !InVision.ContainsKey( player ) )
					InVision.Add( player, 0f );
				else
					InVision[ player ] = 0f;
			}
			else
				if ( InVision.ContainsKey( player ) )
					if ( InVision[player] >= MaxRememberTime ) // Stops following if lost sight after 1 second (This means it will follow you into rooms or behind corridors)
						InVision.Remove( player );
		}

		foreach ( var doob in Entity.All.OfType<Doob>().ToList() )
		{
			if ( IsInVision( doob ) )
			{
				if ( !InVision.ContainsKey( doob ) )
					InVision.Add( doob, 0f );
				else
					InVision[doob] = 0f;
			}
			else
				if ( InVision.ContainsKey( doob ) )
				if ( InVision[doob] >= MaxRememberTime ) // Stops following if lost sight after 1 second (This means it will follow you into rooms or behind corridors)
					InVision.Remove( doob );
		}
	}

	public virtual bool IsInVision( AnimatedEntity entity )
	{
		var visionRange = entity == Target ? MaxVisionRangeWhenChasing : MaxVisionRange;
		var visionAngle = entity == Target ? MaxVisionAngleWhenChasing : MaxVisionAngle;

		if ( entity.Position.Distance( Position ) >= visionRange ) return false;

		var relativePosition = Transform.PointToLocal( entity.Position );
		var angle = Vector3.GetAngle( relativePosition, Vector3.Forward );

		if ( angle > visionAngle / 2f || angle < -visionAngle / 2f ) return false;

		var losTrace = Trace.Ray( Position + Vector3.Up * CollisionCapsule.CenterB.z, entity.Position + Vector3.Up * 64f )
			.Ignore( this )
			.Ignore( entity )
			.Run();

		if ( losTrace.Hit ) return false;

		return true;
	}

	protected List<Cell> currentCells = new List<Cell>();
	internal virtual TimeUntil nextTagsCheck { get; set; } = 0.2f;
	public virtual void AssignNearbyTags()
	{
		if ( CurrentGrid == null ) return;

		if ( nextTagsCheck )
		{
			var nearRadius = 50f;
			var midRadius = 100f;
			var longRadius = 150f;
			var bbox = new BBox( Position, longRadius * 2f );

			foreach ( var oldCell in currentCells.ToList() )
			{
				oldCell.Tags.Remove( "monsterNearRange" );
				oldCell.Tags.Remove( "monsterMidRange" );
				oldCell.Tags.Remove( "monsterLongRange" );

				currentCells.Remove( oldCell );
			}

			foreach ( var nearCell in CurrentGrid.GetCellsInBBox( bbox ) )
			{
				if ( nearCell.Position.Distance( Position ) <= nearRadius )
				{
					nearCell.Tags.Add( "monsterNearRange" );
					currentCells.Add( nearCell );
					continue;
				}

				if ( nearCell.Position.Distance( Position ) <= midRadius )
				{
					nearCell.Tags.Add( "monsterMidRange" );
					currentCells.Add( nearCell );
					continue;
				}

				if ( nearCell.Position.Distance( Position ) <= longRadius )
				{
					nearCell.Tags.Add( "monsterLongRange" );
					currentCells.Add( nearCell );
					continue;
				}
			}

			nextTagsCheck = 0.5f;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( CurrentGrid == null ) return;

		foreach ( var oldCell in currentCells.ToList() )
		{
			oldCell.Tags.Remove( "monsterNearRange" );
			oldCell.Tags.Remove( "monsterMidRange" );
			oldCell.Tags.Remove( "monsterLongRange" );
		}
	}
}
