using Editor;

namespace BrickJam;

[HammerEntity]
[EditorModel( "models/furniture/mansion_furniture/mansion_door.vmdl" )]
public class ShopDoor : UsableEntity
{
	public override float InteractionDuration => 0.8f;
	public override string UseString => CanUse ? "enter the mansion" : "ALL PLAYERS NEED TO BE NEARBY TO PROCEED";
	public override string LockText => "lockpick the mansion door";
	public override bool CanUse => Entity.All.OfType<Player>()
		.Where( x => x.IsAlive )
		.All( x => x.Position.Distance( Position ) <= 300f );
	public override bool StartLocked => true;

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

		MansionGame.NextLevel();
	}
}
