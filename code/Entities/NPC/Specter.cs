namespace BrickJam;

public partial class Specter : NPC
{
	public override string ModelPath { get; set; } = "models/specter/specter.vmdl";
	public override float WalkSpeed { get; set; } = 100f;
	public override float RunSpeed { get; set; } = 320f;

	public Specter() { }
	public Specter( Level level ) : base( level ) { }

	public override void Spawn()
	{
		base.Spawn();

		//PlaySound( "sounds/wega.sound" );
	}


	[ConCmd.Server( "Specter" )]
	public static void SpawnNPC()
	{
		var caller = ConsoleSystem.Caller.Pawn;

		var npc = new Specter( MansionGame.Instance.CurrentLevel );
		npc.Position = caller.Position + caller.Rotation.Forward * 300f;
		npc.Rotation = caller.Rotation;
	}
}
