using Editor;

namespace BrickJam;

[HammerEntity]
[EditorModel( "models/editor/axis_helper_thick.vmdl" )]
public partial class LootSpawner : Entity
{
	// TODO: Change this with whatever we end up using for items. (enum, string id, prefab, string path to prefab/resource, etc)
	[Property]
	public LootPrefab LootToSpawn { get; set; }

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Never;

		if ( Loot.CreateFromGameResource( LootToSpawn, Position, Rotation ) is null )
		{
			Log.Error( $"{this} Couldn't spawn item! item: {LootToSpawn}" );
			return;
		}
	}
}
