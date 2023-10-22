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

	public Doob() { }
	public Doob( Level level ) : base( level ) { }

	public override void Spawn()
	{
		base.Spawn();
	}

	public override void ComputeAnimations()
	{
		SetAnimParameter( "move_x", MathX.Remap( Velocity.WithZ(0).Length, 0f, RunSpeed, 0, 1 ) );
	}

	public override void ComputeIdleAndSeek()
	{
		if ( Target == null )
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
	}

	public override void FindTargets()
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


	[ConCmd.Server( "doob" )]
	public static void SpawnNPC()
	{
		var caller = ConsoleSystem.Caller.Pawn;

		var npc = new Doob( MansionGame.Instance.CurrentLevel );
		npc.Position = caller.Position + caller.Rotation.Forward * 100f;
		npc.Rotation = caller.Rotation;
		npc.Owner = caller as Player;
	}
}
