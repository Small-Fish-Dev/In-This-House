namespace BrickJam;

public class Trapdoor : UseableEntity
{
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/placeholders/placeholder_trapdoor.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		Tags.Add( "solid" );
		Tags.Add( "trapdoor" );
	}

	public override void Use( Player user )
	{
		base.Use( user );

		Mansion.NextLevel();
	}
}
