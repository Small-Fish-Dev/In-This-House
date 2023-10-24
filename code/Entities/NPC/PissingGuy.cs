using GridAStar;

namespace BrickJam;

public partial class PissingGuy : NPC
{
	public override string ModelPath { get; set; } = "models/pissing_guy/pissing_guy.vmdl";
	public override float WalkSpeed { get; set; } = 100f;
	public override float RunSpeed { get; set; } = 100f;
	public override float MaxVisionAngle { get; set; } = 360f;
	public override float MaxVisionRange { get; set; } = 200f;
	public override float MaxVisionRangeWhenChasing { get; set; } = 1024f;
	public override float MaxVisionAngleWhenChasing { get; set; } = 180f;
	public override float MaxRememberTime { get; set; } = 2f;
	public override float KillRange { get; set; } = 40f;

	public Vector3 StartingPosition { get; set; } = Vector3.Zero;
	public Rotation StartingRotation { get; set; } = Rotation.Identity;
	internal Particles pissingParticle { get; set; }
	public override string IdleSound => "";
	public override float IdleVolume => 0f;
	public override string AttackSound => "sounds/piss/pissattack.sound";
	public override float AttackVolume => 3f;

	public PissingGuy() { }
	public PissingGuy( Level level ) : base( level ) { }

	public override void Spawn()
	{
		base.Spawn();

		funny();
	}

	public override void ComputeIdleAndSeek()
	{
		if ( InVision.Count > 0 )
		{
			Target = InVision.Where( x => x.Key != null && x.Key.IsValid() )
				.OrderBy( x => x.Key.Position.Distance( Position ) )
				.FirstOrDefault().Key;
			if ( Target is Player player )
				if ( player.Doob != null )
					Target = player.Doob;

			LastTarget = Target;

			nextIdle = MansionGame.Random.Float( 3f, 6f );
		}
		else
			Target = null;

		if ( Target == null )
		{
			if ( !IsFollowingPath )
			{
				if ( StartingPosition != Vector3.Zero )
				{
					if ( Position.Distance( StartingPosition ) <= 30f )
						Rotation = Rotation.Lerp( Rotation, StartingRotation, Time.Delta * 5f );
					else
					{
						var targetCell = Level.Grid?.GetCell( StartingPosition, false ) ?? null;

						if ( targetCell != null )
							NavigateTo( targetCell );
					}
				}

				LastTarget = null;
			}
		}

		if ( Target != null ) // Kill player is in range
			if ( Target.Position.Distance( Position ) <= KillRange )
			{
				if ( Target is Player player )
					CatchPlayer( player );
				if ( Target is Doob doob )
					CatchDoob( doob );

				piss();
			}
	}

	public override void OnAnimEventFootstep( Vector3 position, int foot, float volume )
	{
		base.OnAnimEventFootstep( position, foot, volume );

		Sound.FromWorld( "sounds/piss/pisstomp.sound", position );
	}

	TimeUntil nextDoor = 0f;
	public override void ComputeOpenDoors()
	{
		if ( nextDoor )
		{
			base.ComputeOpenDoors();
			nextDoor = Game.Random.Float( 0.1f, 0.2f );
		}
	}

	TimeUntil nextFind = 0f;
	public override void FindTargets()
	{
		if ( nextFind )
		{
			base.FindTargets();
			nextFind = Game.Random.Float( 0.1f, 0.2f );
		}
	}

	internal async void piss()
	{
		pissingParticle = Particles.Create( "particles/piss/piss.vpcf", Target.Position );
		await GameTask.Delay( (int)(AttackAnimationDuration * 1000) );
		pissingParticle?.Destroy();
	}

	bool fixing = true;
	// ANIMATINO FIX!!
	private async void funny()
	{
		await GameTask.Delay( 500 );
		SetAnimParameter( "walking", true );
		await GameTask.Delay( 500 );
		SetAnimParameter( "walking", false );
		fixing = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		pissingParticle?.Destroy();
	}

	public override void ComputeAnimations()
	{
		if ( !fixing )
			SetAnimParameter( "walking", Velocity.WithZ(0).Length >= 5f ? true : false );
	}

	public override void AssignNearbyTags()
	{
	}


	[ConCmd.Server( "PissingGuy" )]
	public static void SpawnNPC()
	{
		if ( ConsoleSystem.Caller.Pawn is not Player player ) return;

		var npc = new PissingGuy( MansionGame.Instance.CurrentLevel );
		npc.Position = player.Position + player.Rotation.Forward * 300f;
		npc.Rotation = player.Rotation;
		npc.StartingPosition = npc.Position;
		npc.StartingRotation = npc.Rotation;
	}
}
