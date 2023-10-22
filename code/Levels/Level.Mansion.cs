namespace BrickJam;

public partial class MansionLevel : Level
{
	public override LevelType Type { get; set; } = LevelType.Mansion;
	public override BBox WorldBox => new BBox( new Vector3( -3200f, -2000f, -80f ), new Vector3( 3200f, 2000f, 1700f ) );

	public async override Task Start()
	{
		await base.Start();

		var monster = new AoNyobo( this );
		monster.Position = Exit.Position;
		RegisterMonster( monster );

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
