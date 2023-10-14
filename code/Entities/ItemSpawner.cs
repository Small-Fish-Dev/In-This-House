using Editor;

namespace BrickJam;

[HammerEntity]
[EditorModel( "models/editor/axis_helper_thick.vmdl" )]
public partial class ItemSpawner : Entity
{
	// TODO: Change this with whatever we end up using for items. (enum, string id, prefab, string path to prefab/resource, etc)
	[Property]
	public ItemPrefab ItemToSpawn { get; set; }

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Never;

		if ( Item.CreateFromGameResource( ItemToSpawn, Position, Rotation ) is null )
		{
			Log.Error( $"{this} Couldn't spawn item! item: {ItemToSpawn}" );
			return;
		}
	}
}
