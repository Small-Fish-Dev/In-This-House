namespace BrickJam;

public class Trapdoor : UsableEntity
{
	public override float InteractionDuration => 2.0f;
	public override string UseString => "get to the next level";

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

		MansionGame.NextLevel();
	}
}
