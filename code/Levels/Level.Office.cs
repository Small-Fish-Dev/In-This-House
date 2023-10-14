﻿namespace BrickJam;

public partial class OfficeLevel : Level
{

	public async override Task Start()
	{
		await base.Start();

		foreach ( var player in Entity.All.OfType<Player>() )
			player.Respawn();

		// Spawn monster?

		var allValidTrapdoors = Entity.All.OfType<ValidTrapdoorPosition>()
			.Where( x => x.LevelType == LevelType.Office )
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