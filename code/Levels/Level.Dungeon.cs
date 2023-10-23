namespace BrickJam;

public partial class DungeonLevel : Level
{
	public override LevelType Type { get; set; } = LevelType.Dungeon;
	public override string Music => "sounds/music/malevolent_sightings_in_the_room.sound";
	Vector3 min => new Vector3( 500f, -2700f, -2000f );
	Vector3 max => new Vector3( 3500f, 2000f, -1500f );
	public override BBox WorldBox => new BBox( min, max );

	public async override Task Start()
	{
		await base.Start();

		var monster = new Specter( this );

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
