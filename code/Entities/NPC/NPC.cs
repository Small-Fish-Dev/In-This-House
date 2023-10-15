using GridAStar;

namespace BrickJam;

public partial class NPC : AnimatedEntity
{
	public Level Level { get; private set; }
	internal TimeUntil nextIdle { get; set; } = 0f;
	public virtual string ModelPath { get; set; } = "models/citizen/citizen.vmdl";
	public virtual float CollisionRadius { get; set; } = 12f;
	public virtual float CollisionHeight { get; set; } = 72f;
	public Capsule CollisionCapsule => new Capsule( Vector3.Up * CollisionRadius, Vector3.Up * (CollisionHeight - CollisionRadius), CollisionRadius );
	public virtual float MaxVisionRange { get; set; } = 1024f;
	public virtual float MaxVisionAngle { get; set; } = 120f; // Degrees
	public List<Player> PlayersInVision { get; private set; } = new();
	public Player Target { get; set; } = null;
	public Player LastTarget { get; set; } = null;

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

		if ( PlayersInVision.Count > 0 )
		{
			Target = PlayersInVision.OrderBy( x => x.Position.Distance( Position ) )
				.FirstOrDefault();
			LastTarget = Target;

			nextIdle = Game.Random.Float( 3f, 6f );
		}
		else
			Target = null;

		if ( Target == null )
		{
			if ( nextIdle && !IsFollowingPath )
			{
				var randomPosition = Level.WorldBox.RandomPointInside;
				var targetCell = Level.Grid?.GetCell( randomPosition, false ) ?? null;

				if ( targetCell != null )
				{
					NavigateTo( targetCell );
					nextIdle = Game.Random.Float( 3f, 6f );
				}

				LastTarget = null;
			}
		}


		ComputeNavigation();
		ComputeMotion();
	}

	public virtual void FindTargets()
	{
		foreach ( var player in Entity.All.OfType<Player>() )
		{
			if ( IsPlayerInVision( player ) )
			{
				if ( !PlayersInVision.Contains( player ) )
					PlayersInVision.Add( player );
			}
			else
				if ( PlayersInVision.Contains( player ) )
					PlayersInVision.Remove( player );
		}
	}

	public virtual bool IsPlayerInVision(  Player player )
	{
		if ( player.Position.Distance( Position ) >= MaxVisionRange ) return false;

		var relativePosition = Transform.PointToLocal( player.Position );
		var angle = Vector3.GetAngle( relativePosition, Vector3.Forward );

		if ( angle > MaxVisionAngle / 2f || angle < -MaxVisionAngle / 2f ) return false;

		var losTrace = Trace.Ray( Position + Vector3.Up * CollisionCapsule.CenterB.z, player.Position + Vector3.Up * player.CollisionCapsule.CenterB.z )
			.Ignore( this )
			.Ignore( player )
			.Run();

		if ( losTrace.Hit ) return false;

		return true;
	}
}
