namespace BrickJam;

public partial class MansionLevel : Level
{
	public override LevelType Type { get; set; } = LevelType.Mansion;
	public override string Music => "sounds/music/looming_trees_in_eerie_woods.sound";
	Vector3 min => new Vector3( -3000f, -2600f, -200f );
	Vector3 max => new Vector3( 1200f, 400f, 1200f );
	public override BBox WorldBox => new BBox( min, max );

	public async override Task Start()
	{
		await base.Start();

		var monster = new AoNyobo( this );

		var allValidTrapdoors = Entity.All.OfType<ValidTrapdoorPosition>()
			.Where( x => x.LevelType == Type )
			.ToList();
		var randomValidTrapdoor = MansionGame.Random.FromList( allValidTrapdoors );

		monster.Position = randomValidTrapdoor.Position;
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
