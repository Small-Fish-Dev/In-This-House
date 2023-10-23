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
				v.FrictionMultiplier = 1.5f )
			.WithPrice( 500 )
			// .WithTexture( "ui/icon/jellyfish-jam.png" )
			.Build();

		new Upgrade.Builder( "Cartoony Sidekick", "Summon Doob the Dog to protect you" )
			.ConfigureWith( v =>
				v.Doob = true )
			.WithPrice( 1000 )
			// .WithTexture( "ui/icon/jellyfish-jam.png" )
			.Build();

		new Upgrade.Builder( "Mansion Key", "The key needed to unlock the trapdoor found in the mansion" )
			.ConfigureWith( v =>
				v.KeyToDungeon = true )
			.WithPrice( 800 )
			// .WithTexture( "ui/icon/jellyfish-jam.png" )
			.Build();

		new Upgrade.Builder( "Faster Use", "Interacting with items takes less time" )
			.ConfigureWith( v =>
				v.UseMultiplier = 1.5f )
			.WithPrice( 1100 )
			// .WithTexture( "ui/icon/jellyfish-jam.png" )
			.Build();
		
		new Upgrade.Builder( "Dungeon Key", "The key needed to unlock the trapdoor found in the mansion" )
			.ConfigureWith( v =>
				v.KeyToBathrooms = true )
			.WithPrice( 1600 )
			// .WithTexture( "ui/icon/jellyfish-jam.png" )
			.Build();

		new Upgrade.Builder( "Lock Breaker", "No need to lockpick, just break the lock" )
			.ConfigureWith( v =>
				v.BreakLocks = true )
			.WithPrice( 1800 )
			// .WithTexture( "ui/icon/jellyfish-jam.png" )
			.Build();
		
		new Upgrade.Builder( "Exit Key", "The key needed to exit from this cursed place" )
			.ConfigureWith( v =>
				v.KeyToExit = true )
			.WithPrice( 2200 )
			// .WithTexture( "ui/icon/jellyfish-jam.png" )
			.Build();
	}
}
