using Editor;

namespace BrickJam;

[HammerEntity]
[EditorModel( "models/items/candle_holder/candle_holder.vmdl" )]
public partial class LootSpawner : Entity
{
	[Property]
	public LootPrefab LootToSpawn { get; set; }
	[Property]
	public float ChanceToSpawn { get; set; } = 0.5f;
	[Property]
	public LevelType Level { get; set; } = LevelType.None;
	public Loot LootSpawned { get; set; }

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Never;
	}

	public void SpawnLoot()
	{
		var chance = Game.Random.Float();

		if ( chance <= ChanceToSpawn )
		{
			LootSpawned = Loot.CreateFromGameResource( LootToSpawn, Position, Rotation );

			if ( LootSpawned is null )
				Log.Error( $"{this} Couldn't spawn item! item: {LootToSpawn}" );
		}
	}

	public void DeleteLoot() => LootSpawned?.Delete();
}
