namespace BrickJam;

public partial class BathroomsLevel : Level
{
	public override LevelType Type { get; set; } = LevelType.Bathrooms;
	public override string Music => "sounds/music/depths_and_terror.sound";
	public override BBox WorldBox => new BBox( new Vector3( -3200f, -2000f, -3400f ), new Vector3( 4000f, 3900f, -5400f ) );
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
