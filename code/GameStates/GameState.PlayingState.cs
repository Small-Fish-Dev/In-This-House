namespace BrickJam;

public partial class PlayingState : GameState
{

	public async override Task Start()
	{
		await base.Start();

		foreach ( var player in Entity.All.OfType<Player>() )
		{
			// Respawn
		}

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
