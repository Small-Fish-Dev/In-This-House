namespace BrickJam;

public class Trapdoor : ModelEntity
{
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/placeholders/placeholder_trapdoor.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		Tags.Add( "trapdoor" );
	}
}
