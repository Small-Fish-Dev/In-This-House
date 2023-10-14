namespace BrickJam;

public partial class MansionLevel : Level
{
	public override LevelType Type { get; set; } = LevelType.Mansion;

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

		Trapdoor?.Delete();

		return;
	}
}
