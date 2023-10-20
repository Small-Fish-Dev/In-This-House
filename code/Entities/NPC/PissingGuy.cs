namespace BrickJam;

public partial class PissingGuy : NPC
{
	public override string ModelPath { get; set; } = "models/pissing_guy/pissing_guy.vmdl";
	public override float WalkSpeed { get; set; } = 100f;
	public override float RunSpeed { get; set; } = 140f;
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
	public override void Think()
	{
		FindTargets();

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
			if ( Target.Position.Distance( Position ) <= 40f )
				CatchPlayer( Target );

		if ( IsFollowingPath )
			foreach ( var door in Entity.All.OfType<Door>() )
			{
				if ( door.Position.Distance( Position ) <= 60f )
					if ( door.State == DoorState.Closed )
						door.State = DoorState.Open;
			}

		ComputeOpenDoors();
		ComputeNavigation();
		ComputeMotion();
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
