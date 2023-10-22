using GridAStar;

namespace BrickJam;

public partial class PissingGuy : NPC
{
	public override string ModelPath { get; set; } = "models/pissing_guy/pissing_guy.vmdl";
	public override float WalkSpeed { get; set; } = 80f;
	public override float RunSpeed { get; set; } = 120f;
	public override float MaxVisionAngle { get; set; } = 360f;
	public override float MaxVisionRange { get; set; } = 150f;
	public override float MaxVisionRangeWhenChasing { get; set; } = 1024f;
	public override float MaxVisionAngleWhenChasing { get; set; } = 180f;
	public override float MaxRememberTime { get; set; } = 3f;

	Vector3 startingPosition = Vector3.Zero;
	Rotation startingRotation = Rotation.Identity;

	public PissingGuy() { }
	public PissingGuy( Level level ) : base( level ) { }

	public override void Spawn()
	{
		base.Spawn();

		//PlaySound( "sounds/wega.sound" );
	}

	public override void ComputeIdleAndSeek()
	{

		if ( PlayersInVision.Count > 0 )
		{
			Target = PlayersInVision.OrderBy( x => x.Key.Position.Distance( Position ) )
				.FirstOrDefault().Key;
			LastTarget = Target;

			if ( startingPosition == Vector3.Zero )
				startingPosition = Position;

			if ( startingRotation == Rotation.Identity )
				startingRotation = Rotation;

			nextIdle = MansionGame.Random.Float( 3f, 6f );
		}
		else
			Target = null;

		if ( Target == null )
		{
			if ( !IsFollowingPath )
			{
				if ( startingPosition != Vector3.Zero )
				{
					if ( Position.Distance( startingPosition ) <= 30f )
						Rotation = Rotation.Lerp( Rotation, startingRotation, Time.Delta * 5f );
					else
					{
						var targetCell = Level.Grid?.GetCell( startingPosition, false ) ?? null;

						if ( targetCell != null )
							NavigateTo( targetCell );
					}
				}

				LastTarget = null;
			}
		}

		if ( Target != null ) // Kill player is in range
			if ( Target.Position.Distance( Position ) <= KillRange )
				CatchPlayer( Target );

		if ( IsFollowingPath )
			foreach ( var door in Entity.All.OfType<Door>() )
			{
				if ( door.Position.Distance( Position ) <= 60f )
					if ( door.State == DoorState.Closed )
						door.State = DoorState.Open;
			}
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
		var caller = ConsoleSystem.Caller.Pawn;

		var npc = new PissingGuy( MansionGame.Instance.CurrentLevel );
		npc.Position = caller.Position + caller.Rotation.Forward * 300f;
		npc.Rotation = caller.Rotation;
	}
}
