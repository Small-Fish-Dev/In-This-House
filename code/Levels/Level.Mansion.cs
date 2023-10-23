namespace BrickJam;

public partial class MansionLevel : Level
{
	public override LevelType Type { get; set; } = LevelType.Mansion;
	public override string Music => "sounds/music/looming_trees_in_eerie_woods.sound";
	Vector3 pos => new Vector3( -368f, -3.99976f, 683.625f );
	Vector3 size => new Vector3( 4000f, 2700f, 400f );
	public override BBox WorldBox => new BBox( pos - size / 2f, pos + size / 2f );

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
