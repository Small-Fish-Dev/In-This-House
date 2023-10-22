using Sandbox.Utility;

namespace BrickJam;

public partial class Specter : NPC
{
	public override string ModelPath { get; set; } = "models/specter/specter.vmdl";
	public override float WalkSpeed { get; set; } = 100f;
	public override float RunSpeed { get; set; } = 320f;
	internal CapsuleLightEntity lampLight { get; set; }

	public Specter() { }
	public Specter( Level level ) : base( level ) { }

	public override void ClientSpawn()
	{
		base.Spawn();

		//PlaySound( "sounds/wega.sound" );

		lampLight = new CapsuleLightEntity();
		lampLight.CapsuleLength = 12f;
		lampLight.LightSize = 12f;
		lampLight.Enabled = true;
		lampLight.Color = new Color( 0.5f, 0.4f, 0.2f );
		lampLight.Range = 512f;
		lampLight.Brightness = 0.4f;
		lampLight.Position = GetAttachment( "lamp" ).Value.Position;
		lampLight.SetParent( this, "lamp" );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		lampLight.Delete();
	}

	[GameEvent.Tick.Client]
	void flickerLight()
	{
		if ( lampLight != null )
			lampLight.Brightness = Noise.Perlin( Time.Now * 200f, 100f, 1000f );
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
