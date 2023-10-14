namespace BrickJam;

public partial class DungeonLevel : Level
{
	public override LevelType Type { get; set; } = LevelType.Dungeon;
	public override BBox WorldBox => new BBox( new Vector3( -1550f, -1550f, -640f ), new Vector3( 1550f, 1550f, -500f ) );

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
