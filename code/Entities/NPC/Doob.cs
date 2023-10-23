using GridAStar;

namespace BrickJam;

public partial class Doob : NPC
{
	public override string ModelPath { get; set; } = "models/doob/doob.vmdl";
	public override float WalkSpeed { get; set; } = 140f;
	public override float RunSpeed { get; set; } = 380f;
	public override float CollisionHeight { get; set; } = 24f;
	public override float CollisionRadius { get; set; } = 10f;
	public bool IsBeingChased { get; set; } = false;
	public new Player Owner { get; set; } = null;
	public new float PushForce { get; set; } = 500f;
	public override float WishSpeed => Direction.IsNearlyZero() ? 0 : (HasArrivedDestination ? 0f : (IsBeingChased ? RunSpeed : WalkSpeed));
	public override string IdleSound => "sounds/dooblaugh.sound";
	public override float IdleVolume => 1.5f;
	public override string AttackSound => "";
	public override float AttackVolume => 2f;

	internal Sound runningSound { get; set; }
	public Doob() { }
	public Doob( Level level ) : base( level ) { }

	public override void Spawn()
	{
		base.Spawn();
		Tags.Add( "doob" );
	}

	public override void ComputeAnimations()
	{
		SetAnimParameter( "move_x", MathX.Remap( Velocity.WithZ(0).Length, 0f, RunSpeed, 0, 3 ) );
	}

	public override void ComputeIdleAndSeek()
	{
		if ( !IsFollowingPath )
		{
			if ( nextIdle )
			{
				if ( !IsBeingChased )
				{
					var randomDirection = MansionGame.Random.VectorInCircle( 80f );
					var closestCell = Level.Grid?.GetNearestCell( Owner.Position + new Vector3( randomDirection.x, randomDirection.y, 50 ), false ) ?? null;

					if ( closestCell != null )
						NavigateTo( closestCell );

					nextIdle = MansionGame.Random.Float( 0.5f, 1f );

					if ( runningSound.IsPlaying )
						runningSound.Stop();
				}
				else
				{
					if ( !runningSound.IsPlaying )
						runningSound = PlaySound( "sounds/running.sound" );

					var closestMonster = Entity.All.OfType<NPC>().Where( x => x.Target == this ).FirstOrDefault();

					if ( closestMonster == null ) return;

					Cell chosenCell = null;
					var tried = 0;

					while ( chosenCell == null )
					{
						tried++;

						if ( tried >= 40 ) // We get desperate after 20 tries so we accept worse positions
							break;

						var randomCell = MansionGame.Random.FromList( Level.Grid?.AllCells.ToList() ) ?? null;

						if ( randomCell.Position.Distance( closestMonster.Position ) >= ( tried > 20 ? 300f : 600f ) )
						{
							var relativePosition = closestMonster.Transform.PointToLocal( randomCell.Position );
							var angle = Vector3.GetAngle( relativePosition.WithZ(0), Vector3.Forward );

							var acceptableAngle = tried > 20 ? 90f : 140f;

							if ( angle <= acceptableAngle && angle >= -acceptableAngle ) // Only pick cells that are on doob's side, not the monster's
							{
								chosenCell = randomCell;
							}
						}

						
					}

					if ( chosenCell != null )
						NavigateTo( chosenCell );

					nextIdle = MansionGame.Random.Float( 0.3f, 0.6f );
				}
			}
		}


		if ( nextIdleSound )
		{
			var idle = PlaySound( IdleSound );
			idle.SetVolume( IdleVolume );
		}
	}
	public override AStarPathBuilder PathBuilder => new AStarPathBuilder( CurrentGrid )
		.WithPathCreator( this )
		.WithPartialEnabled()
		.WithMaxDistance( 2000f )
		.AvoidTag( "door", 400f )
		.AvoidTag( "edge", 50f )
		.AvoidTag( "outeredge", 60f )
		.AvoidTag( "inneredge", 40f )
		.AvoidTag( "monsterNearRange", 200f )
		.AvoidTag( "monsterMidRange", 100f )
		.AvoidTag( "monsterLongRange", 50f );

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

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( runningSound.IsPlaying )
			runningSound.Stop();
	}

	bool isDead = false;
	public void Kill()
	{
		if ( isDead || !IsValid ) return;

		isDead = true;

		Particles.Create( "particles/blood/blood_explosion.vpcf", Position );
		Sound.FromWorld( "sounds/death.sound", Position );

		if ( runningSound.IsPlaying )
			runningSound.Stop();

		DeleteAsync( 0.1f );
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
