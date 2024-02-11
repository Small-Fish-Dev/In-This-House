namespace BrickJam;

public partial class ShopLevel : Level
{
	public override LevelType Type { get; set; } = LevelType.Shop;
	public override string Music => "sounds/music/scary_quest_at_midnight.sound";

	public async override Task Start()
	{
		// We don't do base.start here

		RespawnAll();

		foreach ( var player in Entity.All.OfType<Player>().Where( p => p.Client is null ) )
			player.Delete();

		MansionGame.Instance.TimerStop();

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
