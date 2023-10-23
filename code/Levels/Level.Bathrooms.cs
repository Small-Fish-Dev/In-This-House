namespace BrickJam;

public partial class BathroomsLevel : Level
{
	public override LevelType Type { get; set; } = LevelType.Bathrooms;
	public override string Music => "sounds/music/depths_and_terror.sound";
	Vector3 pos => new Vector3( 448f, 704.75f, -4800f );
	Vector3 size => new Vector3( 6600f, 4800f, 200f );
	public override BBox WorldBox => new BBox( pos - size / 2f, pos + size / 2f);
	public async override Task Start()
	{
		await base.Start();

		foreach ( var spawner in Entity.All.OfType<PissingGuySpawner>().ToList() )
		{
			var guy = spawner.SpawnGuy();

			if ( guy != null )
				RegisterMonster( guy );
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
