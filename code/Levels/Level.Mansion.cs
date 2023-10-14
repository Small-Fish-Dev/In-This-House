namespace BrickJam;

public partial class MansionLevel : Level
{

	public async override Task Start()
	{
		await base.Start();

		// Spawn monster?

		var allValidTrapdoors = Entity.All.OfType<ValidTrapdoorPosition>()
			.Where( x => x.LevelType == LevelType.Mansion )
			.ToList();
		var randomValidTrapdoor = Game.Random.FromList( allValidTrapdoors );

		Trapdoor = new Trapdoor();
		Trapdoor.Position = randomValidTrapdoor.Position;

		return;
	}

	public override void Compute()
	{
		base.Compute();
	}

	public async override Task End()
	{
		await base.End();

		return;
	}
}
