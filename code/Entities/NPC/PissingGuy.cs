namespace BrickJam;

public partial class PissingGuy : NPC
{
	public override string ModelPath { get; set; } = "models/pissing_guy/pissing_guy.vmdl";
	public override float WalkSpeed { get; set; } = 100f;
	public override float RunSpeed { get; set; } = 140f;

	public PissingGuy() { }
	public PissingGuy( Level level ) : base( level ) { }

	public override void Spawn()
	{
		base.Spawn();
		Scale = 1.1f;

		//PlaySound( "sounds/wega.sound" );
	}


	[ConCmd.Server( "PissingGuy" )]
	public static void SpawnNPC()
	{
		var caller = ConsoleSystem.Caller.Pawn;

		var npc = new PissingGuy( MansionGame.Instance.CurrentLevel );
		npc.Position = caller.Position + caller.Rotation.Forward * 100f;
		npc.Rotation = caller.Rotation;
	}
}
