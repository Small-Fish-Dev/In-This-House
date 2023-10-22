using GridAStar;

namespace BrickJam;

public partial class Doob : NPC
{
	public override string ModelPath { get; set; } = "models/doob/doob.vmdl";
	public override float WalkSpeed { get; set; } = 180f;
	public override float RunSpeed { get; set; } = 360f;
	public override float CollisionHeight { get; set; } = 34f;
	public override float CollisionRadius { get; set; } = 16f;
	public bool IsBeingChased { get; set; } = false;
	public new Player Owner { get; set; } = null;
	public new float PushForce { get; set; } = 1000f;

	public Doob() { }
	public Doob( Level level ) : base( level ) { }

	public override void Spawn()
	{
		base.Spawn();
		Tags.Add( "doob" );
	}

	public override void ComputeAnimations()
	{
		SetAnimParameter( "move_x", MathX.Remap( Velocity.WithZ(0).Length, 0f, RunSpeed, 0, 1 ) );
	}

	public override void ComputeIdleAndSeek()
	{
		if ( !IsFollowingPath )
		{
			if ( nextIdle )
			{
				var randomDirection = MansionGame.Random.VectorInCircle( 80f );
				var closestCell = Level.Grid?.GetNearestCell( Owner.Position + new Vector3( randomDirection.x, randomDirection.y, 50 ), false ) ?? null;

				if ( closestCell != null )
					NavigateTo( closestCell );

				nextIdle = MansionGame.Random.Float( 0.5f, 1f );
			}
		}
	}
	public override AStarPathBuilder PathBuilder => new AStarPathBuilder( CurrentGrid )
		.WithPathCreator( this )
		.WithPartialEnabled()
		.WithMaxDistance( 2000f )
		.AvoidTag( "door", 400f )
		.AvoidTag( "edge", 50f )
		.AvoidTag( "outeredge", 40f )
		.AvoidTag( "inneredge", 30f )
		.AvoidTag( "monsterNearRange", 50f )
		.AvoidTag( "monsterMidRange", 30f )
		.AvoidTag( "monsterLongRange", 10f );

	public override void FindTargets()
	{
	}

	public override void AssignNearbyTags()
	{
	}

	public override void ComputeMotion()
	{
		base.ComputeMotion();
	}

	public override void ComputeOpenDoors()
	{
		base.ComputeOpenDoors();
	}

	public override void Think()
	{
		base.Think();

		IsBeingChased = Entity.All.OfType<NPC>().Any( x => x.Target == this );
	}

	public void Kill()
	{
		DeleteAsync( 1f ); // TODO KILL
	}


	[ConCmd.Server( "doob" )]
	public static void SpawnNPC()
	{
		if ( ConsoleSystem.Caller.Pawn is not Player player ) return;

		var npc = new Doob( MansionGame.Instance.CurrentLevel );
		npc.Position = player.Position + player.Rotation.Forward * 100f;
		npc.Rotation = player.Rotation;
		npc.Owner = player;
		player.Doob = npc;
	}
}
