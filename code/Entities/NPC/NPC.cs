using GridAStar;

namespace BrickJam;

public partial class NPC : AnimatedEntity
{
	public Level Level { get; private set; }
	internal TimeUntil nextIdle { get; set; } = 0f;
	public virtual string ModelPath { get; set; } = "models/citizen/citizen.vmdl";
	public virtual BBox CollisionBox { get; set; } = new( new Vector3( -6f, -6f, 0f ), new Vector3( 6f, 6f, 72f ) );

	public NPC() { }
	public NPC( Level level ) : base()
	{
		Level = level;
	}

	public override void Spawn()
	{
		base.Spawn();

		SetModel( ModelPath );
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, CollisionBox.Mins, CollisionBox.Maxs );
		Tags.Add( "npc" );
	}

	[GameEvent.Tick.Server]
	public virtual void Think()
	{
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
			}

			//foreach ( var player in Entity.All.OfType<Player>() ) // TODO: Line of Sight

			//Target = Entity.All.OfType<Player>().First();
		}


		ComputeNavigation();
		ComputeMotion();
	}
}
