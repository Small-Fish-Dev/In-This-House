namespace BrickJam;

public partial class ShopLevel : Level
{
	public override LevelType Type { get; set; } = LevelType.Shop;

	public async override Task Start()
	{
		//await base.Start(); // Let's do this manually here
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
