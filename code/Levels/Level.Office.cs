namespace BrickJam;

public partial class BathroomsLevel : Level
{
	public override LevelType Type { get; set; } = LevelType.Bathrooms;
	public override BBox WorldBox => new BBox( new Vector3( -1550f, -1550f, -1660f ), new Vector3( 1550f, 1550f, -1520f ) );

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
