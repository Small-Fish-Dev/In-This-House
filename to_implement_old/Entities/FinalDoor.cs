using Editor;

namespace BrickJam;

public class FinalDoor : UsableEntity
{
	public override float InteractionDuration => 1.7f;
	public override string UseString => HasKey ? (CanUse ? "exit the mansion" : "ALL PLAYERS NEED TO BE NEARBY TO PROCEED") : "YOU NEED TO BUY THE KEY TO PROCEED";
	public override bool CanUse => Entity.All.OfType<Player>()
		.Where( x => x.IsAlive )
		.All( x => x.Position.Distance( Position ) <= 400f ) && (Game.IsClient ? HasKey : true);
	public bool HasKey => (Game.IsClient ? Game.LocalPawn as Player : User)?.HasUpgrade( "Exit Key" ) ?? false;


	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/furniture/exit_door.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		Tags.Add( "door" );
	}
	
	public override void Use( Player user )
	{
		base.Use( user );
		MansionGame.RestartGame(); // TODO: Give bonus / Show ending
	}
	public override bool CheckUpgrades( Player player )
	{
		return player.HasUpgrade( "Exit Key" );
	}
}
