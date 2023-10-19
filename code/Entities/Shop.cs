using Editor;

namespace BrickJam;

[HammerEntity]
[EditorModel( "models/containers/chest/chest.vmdl" )]
public class Shop : UsableEntity
{
	public override float InteractionDuration => 0f;
	public override string UseString => "open the upgrades shop";

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/containers/chest/chest.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		Tags.Add( "solid" );
	}

	public override void Use( Player user )
	{
		base.Use( user );

		// OPEN THE SHOPPE
	}
}
