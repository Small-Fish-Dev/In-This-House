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
			// .WithTexture( "ui/icon/jellyfish-jam.png" )
			.Build();

		new Upgrade.Builder( "Cartoony Sidekick", "Summon Doob the Dog to protect you" )
			.ConfigureWith( v =>
				v.Doob = 1f )
			.WithPrice( 1000 )
			// .WithTexture( "ui/icon/jellyfish-jam.png" )
			.Build();

		new Upgrade.Builder( "Mansion Key", "The key needed to unlock the trapdoor found in the mansion" )
			.ConfigureWith( v =>
				v.KeyToDungeon = 1f )
			.WithPrice( 800 )
			// .WithTexture( "ui/icon/jellyfish-jam.png" )
			.Build();

		new Upgrade.Builder( "Faster Use", "Interacting with items takes less time" )
			.ConfigureWith( v =>
				v.UseMultiplier = 2f )
			.WithPrice( 1100 )
			// .WithTexture( "ui/icon/jellyfish-jam.png" )
			.Build();
		
		new Upgrade.Builder( "Dungeon Key", "The key needed to unlock the trapdoor found in the mansion" )
			.ConfigureWith( v =>
				v.KeyToBathrooms = 1f )
			.WithPrice( 1600 )
			// .WithTexture( "ui/icon/jellyfish-jam.png" )
			.Build();

		new Upgrade.Builder( "Lock Breaker", "No need to lockpick, just break the lock" )
			.ConfigureWith( v =>
				v.BreakLocks = 1f )
			.WithPrice( 1800 )
			// .WithTexture( "ui/icon/jellyfish-jam.png" )
			.Build();
		
		new Upgrade.Builder( "Exit Key", "The key needed to exit from this cursed place" )
			.ConfigureWith( v =>
				v.KeyToExit = 1f )
			.WithPrice( 2200 )
			// .WithTexture( "ui/icon/jellyfish-jam.png" )
			.Build();
	}
}
