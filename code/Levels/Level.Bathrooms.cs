namespace BrickJam;

public partial class BathroomsLevel : Level
{
	public override LevelType Type { get; set; } = LevelType.Bathrooms;
	public override string Music => "sounds/music/depths_and_terror.sound";
	Vector3 min => new Vector3( -2600f, -2100f, -4900f );
	Vector3 max => new Vector3( 4400f, 2900f, -4500f );
	public override BBox WorldBox => new BBox( min, max );
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
