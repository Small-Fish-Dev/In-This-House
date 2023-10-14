using GridAStar;

namespace BrickJam;

public partial class NPC : AnimatedEntity
{
	public Level Level { get; private set; }
	public Vector2 TimeBetweenIdleMove => new Vector2( 2f, 4f );
	internal TimeUntil nextIdleMode { get; set; } = 0f;
	public virtual string ModelPath { get; set; } = "models/citizen/citizen.vmdl";
	public virtual BBox CollisionBox { get; set; } = new( new Vector3( -12f, -12f, 0f ), new Vector3( 12f, 12f, 72f ) );

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

	[GameEvent.Tick]
	public virtual void Think()
	{
		if ( Target == null )
		{
			//var random = Vector3.Random;
			//var targetPosition = random * 3000;
			//var targetCell = CurrentGrid.GetCell( targetPosition, false );

			//NavigateTo( CurrentGrid.GetCell( Entity.All.OfType<Player>().First().Position ) );
			//nextIdleMode = Game.Random.Float( TimeBetweenIdleMove.x, TimeBetweenIdleMove.y );

			Target = Entity.All.OfType<Player>().First();
		}


		ComputeNavigation();
		ComputeMotion();
	}
}
