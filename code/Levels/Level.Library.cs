namespace BrickJam;

public partial class LibraryLevel : Level
{
	public override LevelType Type { get; set; } = LevelType.Library;

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
