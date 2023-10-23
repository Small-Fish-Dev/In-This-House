namespace BrickJam;

public class Trapdoor : UsableEntity
{
	public override float InteractionDuration => 2.0f;
	public override string UseString => HasKey ? ( CanUse ? "proceed to the next level" : "ALL PLAYERS NEED TO BE NEARBY TO PROCEED" ) : "YOU NEED TO BUY THE KEY TO PROCEED";
	public override bool CanUse => Entity.All.OfType<Player>()
		.Where( x => x.IsAlive )
		.All( x => x.Position.Distance( Position ) <= 400f ) && ( Game.IsClient ? HasKey : true );
	public bool HasKey => (Game.IsClient ? Game.LocalPawn as Player : User)?.HasUpgrade( MansionGame.Instance.CurrentLevel.Type == LevelType.Mansion ? "Mansion Key" : "Dungeon Key" ) ?? false;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/furniture/trap_door.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		Tags.Add( "solid" );
		Tags.Add( "trapdoor" );
	}

	public override void Use( Player user )
	{
		base.Use( user );

		if ( CanUse )
		{
			MansionGame.NextLevel();
			PlaySound( "sounds/doors/dooropen.sound" );
		}
	}

	public override bool CheckUpgrades( Player player )
	{
		return player.HasUpgrade( MansionGame.Instance.CurrentLevel.Type == LevelType.Mansion ? "Mansion Key" : "Dungeon Key" );
	}
}
