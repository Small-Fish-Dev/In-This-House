namespace BrickJam;

public partial class LibraryLevel : Level
{
	public override LevelType Type { get; set; } = LevelType.Library;

	public async override Task Start()
	{
		await base.Start();

		foreach ( var player in Entity.All.OfType<Player>() )
			player.Respawn();

		// Spawn monster?

		var allValidTrapdoors = Entity.All.OfType<ValidTrapdoorPosition>()
			.Where( x => x.LevelType == LevelType.Library )
			.ToList();
		var randomValidTrapdoor = Game.Random.FromList( allValidTrapdoors );

		Trapdoor = new Trapdoor();
		Trapdoor.Position = randomValidTrapdoor.Position;

		await GameTask.Delay( 2000 );
		BlackScreen.Start( To.Everyone );

		return;
	}

	public override void Compute()
	{
		base.Compute();
	}

	public async override Task End()
	{
		await base.End();

		Trapdoor?.Delete();

		return;
	}
}
