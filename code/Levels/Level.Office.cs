namespace BrickJam;

public partial class OfficeLevel : Level
{
	public override LevelType Type { get; set; } = LevelType.Office;

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
