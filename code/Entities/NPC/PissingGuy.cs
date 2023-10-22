using GridAStar;

namespace BrickJam;

public partial class PissingGuy : NPC
{
	public override string ModelPath { get; set; } = "models/pissing_guy/pissing_guy.vmdl";
	public override float WalkSpeed { get; set; } = 100f;
	public override float RunSpeed { get; set; } = 100f;
	public override float MaxVisionAngle { get; set; } = 360f;
	public override float MaxVisionRange { get; set; } = 160f;
	public override float MaxVisionRangeWhenChasing { get; set; } = 512f;
	public override float MaxVisionAngleWhenChasing { get; set; } = 180f;
	public override float MaxRememberTime { get; set; } = 2f;
	public override float KillRange { get; set; } = 40f;

	public Vector3 StartingPosition { get; set; } = Vector3.Zero;
	public Rotation StartingRotation { get; set; } = Rotation.Identity;

	public PissingGuy() { }
	public PissingGuy( Level level ) : base( level ) { }

	public override void Spawn()
	{
		base.Spawn();

		//PlaySound( "sounds/wega.sound" );
	}

	public override void ComputeIdleAndSeek()
	{

		if ( InVision.Count > 0 )
		{
			Target = InVision.OrderBy( x => x.Key.Position.Distance( Position ) )
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
			}
	}
	public override void ComputeAnimations()
	{
		SetAnimParameter( "walking", Velocity.WithZ(0).Length >= 5f ? true : false );
	}

	public override void AssignNearbyTags()
	{
		if ( CurrentGrid == null ) return;

		if ( nextTagsCheck )
		{
			var nearRadius = 50f;
			var bbox = new BBox( Position, nearRadius * 2f );

			foreach ( var oldCell in currentCells.ToList() )
			{
				oldCell.Tags.Remove( "monsterNearRange" );

				currentCells.Remove( oldCell );
			}

			foreach ( var nearCell in CurrentGrid.GetCellsInBBox( bbox ) )
			{
				if ( nearCell.Position.Distance( Position ) <= nearRadius )
				{
					nearCell.Tags.Add( "monsterNearRange" );
					currentCells.Add( nearCell );
				}
			}

			nextTagsCheck = 0.5f + Game.Random.Float( -0.1f, 0.1f ); // Gotta add some randomness with these guys or else they'll all do this at the same tick which gets expensive
		}
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
