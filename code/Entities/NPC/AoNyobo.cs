﻿namespace BrickJam;

public partial class AoNyobo : NPC
{
	public override string ModelPath { get; set; } = "models/nyobo/nyobo.vmdl";
	public override float WalkSpeed { get; set; } = 80f;
	public override float RunSpeed { get; set; } = 260f;
	public override float MaxVisionAngle { get; set; } = 240f;
	public override float MaxVisionRange { get; set; } = 700f;
	public override float MaxVisionRangeWhenChasing { get; set; } = 600f;
	public override float MaxVisionAngleWhenChasing { get; set; } = 180f;

	public AoNyobo() { }
	public AoNyobo( Level level ) : base( level ) { }

	public override void Spawn()
	{
		base.Spawn();
		Scale = 1.1f;

		//PlaySound( "sounds/wega.sound" );
	}


	[ConCmd.Server( "AoNyobo" )]
	public static void SpawnNPC()
	{
		if ( ConsoleSystem.Caller.Pawn is not Player player ) return;

		var npc = new AoNyobo( MansionGame.Instance.CurrentLevel );
		npc.Position = player.Position + player.Rotation.Forward * 300f;
		npc.Rotation = player.Rotation;
	}
}
