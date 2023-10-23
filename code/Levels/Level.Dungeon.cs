namespace BrickJam;

public partial class DungeonLevel : Level
{
	public override LevelType Type { get; set; } = LevelType.Dungeon;
	public override string Music => "sounds/music/malevolent_sightings_in_the_room.sound";
	public override BBox WorldBox => new BBox( new Vector3( -3200f, -2000f, -2700f ), new Vector3( 3200f, 2000f, -600f ) );

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
