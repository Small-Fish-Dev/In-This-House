namespace BrickJam;

public partial class DungeonLevel : Level
{
	public override LevelType Type { get; set; } = LevelType.Dungeon;

	public async override Task Start()
	{
		await base.Start();
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
