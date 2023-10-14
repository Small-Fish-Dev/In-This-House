namespace BrickJam;

public partial class MansionLevel : Level
{
	public override LevelType Type { get; set; } = LevelType.Mansion;
	public override BBox WorldBox => new BBox( new Vector3( -1550f, -1550f, 500f ), new Vector3( 1550f, 1550f, 640f ) );

	public async override Task Start()
	{
		await base.Start();

		Monster = new AoNyobo( this );
		Monster.Position = Trapdoor.Position;
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
