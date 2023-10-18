namespace BrickJam;

public class Trapdoor : UsableEntity
{
	public override float InteractionDuration => 2.0f;
	public override string UseString => CanUse ? "proceed to the next level" : "ALL PLAYERS NEED TO BE NEARBY TO PROCEED";
	public override bool CanUse => Entity.All.OfType<Player>()
		.Where( x => x.IsAlive )
		.All( x => x.Position.Distance( Position ) <= 400f );

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
