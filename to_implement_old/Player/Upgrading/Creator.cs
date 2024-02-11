namespace BrickJam.Upgrading;

public static class Creator
{
	static Creator() => Build();

	[Event.Hotload]
	internal static void Build()
	{
		Upgrade.ClearAll();

		new Upgrade.Builder( "Work Shoes", "Makes you slip less" )
			.ConfigureWith( v =>
				v.FrictionMultiplier = 2f )
			.WithPrice( 500 )
			.WithTexture( "ui/upgrades/work_shoes.png", true )
			.Build();

		new Upgrade.Builder( "Cartoony Sidekick", "Summon Doob the Dog to protect you" )
			.ConfigureWith( v =>
				v.Doob = 1f )
			.WithPrice( 1000 )
			.WithTexture( "ui/upgrades/doob.png", true )
			.Build();

		new Upgrade.Builder( "Mansion Key", "The key needed to unlock the trapdoor found in the mansion" )
			.ConfigureWith( v =>
				v.KeyToDungeon = 1f )
			.WithPrice( 800 )
			.WithTexture( "ui/upgrades/mansionkey.png", true )
			.Build();

		new Upgrade.Builder( "Faster Use", "Interacting with items takes less time" )
			.ConfigureWith( v =>
				v.UseMultiplier = 2f )
			.WithPrice( 1100 )
			.WithTexture( "ui/upgrades/fasteruse.png", true )
			.Build();
		
		new Upgrade.Builder( "Dungeon Key", "The key needed to unlock the trapdoor found in the dungeon" )
			.ConfigureWith( v =>
				v.KeyToBathrooms = 1f )
			.WithPrice( 1800 )
			.WithTexture( "ui/upgrades/dungeonkey.png", true )
			.Build();

		new Upgrade.Builder( "Lock Breaker", "No need to lockpick, just break the lock" )
			.ConfigureWith( v =>
				v.BreakLocks = 1f )
			.WithPrice( 2400 )
			.WithTexture( "ui/upgrades/lockbreaker.png", true )
			.Build();
		
		new Upgrade.Builder( "Exit Key", "The key needed to exit from this cursed place" )
			.ConfigureWith( v =>
				v.KeyToExit = 1f )
			.WithPrice( 4400 )
			.WithTexture( "ui/upgrades/exitkey.png", true )
			.Build();
	}
}
