namespace BrickJam;

public partial class DungeonLevel : Level
{
	public override LevelType Type { get; set; } = LevelType.Dungeon;
	public override string Music => "sounds/music/malevolent_sightings_in_the_room.sound";
	Vector3 pos => new Vector3( 1032f, 384f, -1908f );
	Vector3 size => new Vector3( 3100f, 4200f, 400f );
	public override BBox WorldBox => new BBox( pos - size / 2f, pos + size / 2f );

	public async override Task Start()
	{
		await base.Start();

		var monster = new Specter( this );
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
