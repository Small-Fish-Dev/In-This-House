namespace BrickJam;

public partial class AoNyobo : NPC
{
	public override string ModelPath { get; set; } = "models/nyobo/nyobo.vmdl";
	public override BBox CollisionBox { get; set; } = new( new Vector3( -12f, -12f, 0f ), new Vector3( 12f, 12f, 72f ) );
	public override float WalkSpeed { get; set; } = 200f;
	public override float RunSpeed { get; set; } = 300f;

	public AoNyobo() { }
	public AoNyobo( Level level ) : base( level ) { }


	[ConCmd.Server( "SpawnNPC" )]
	public static void SpawnNPC()
	{
		var caller = ConsoleSystem.Caller.Pawn;

		var npc = new AoNyobo( MansionGame.Instance.CurrentLevel );
		npc.Position = caller.Position;
	}
}
