using Editor;

namespace BrickJam;

public class FinalDoor : UsableEntity
{
	public override float InteractionDuration => 1.7f;
	public override string UseString => CanUse ? "exit the mansion" : "ALL PLAYERS NEED TO BE NEARBY TO PROCEED";
	public override bool CanUse => Entity.All.OfType<Player>()
		.Where( x => x.IsAlive )
		.All( x => x.Position.Distance( Position ) <= 300f );

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/furniture/mansion_furniture/mansion_door.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		Tags.Add( "door" );
	}
	
	public override void Use( Player user )
	{
		base.Use( user );
		MansionGame.RestartGame(); // TODO: Give bonus / Show ending
	}
}
