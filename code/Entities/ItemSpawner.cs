using Editor;

namespace BrickJam;

[HammerEntity]
[EditorModel( "models/editor/axis_helper_thick.vmdl" )]
public partial class ItemSpawner : Entity
{
	// TODO: Change this with whatever we end up using for items. (enum, string id, prefab, string path to prefab/resource, etc)
	[Property]
	public ItemPrefab ItemToSpawn { get; set; }

	public static Dictionary<LevelType, Dictionary<ItemRarity, float>> RarityChances { get; set; } = new()
	{
		{
			LevelType.Mansion, new Dictionary<ItemRarity, float>()
			{
				{ ItemRarity.Broken, 1f },
				{ ItemRarity.Decrepit, 1.2f },
				{ ItemRarity.Worn, 1.4f },
				{ ItemRarity.Dusty, 1.2f },
				{ ItemRarity.Common, 1f },
				{ ItemRarity.Nice, 0.6f },
				{ ItemRarity.Great, 0.3f },
				{ ItemRarity.Excellent, 0.1f },
				{ ItemRarity.Flawless, 0.03f }
			}
		},
		{
			LevelType.Dungeon, new Dictionary<ItemRarity, float>()
			{
				{ ItemRarity.Broken, 0.5f },
				{ ItemRarity.Decrepit, 0.7f },
				{ ItemRarity.Worn, 0.9f },
				{ ItemRarity.Dusty, 1f },
				{ ItemRarity.Common, 1.2f },
				{ ItemRarity.Nice, 1f },
				{ ItemRarity.Great, 0.7f },
				{ ItemRarity.Excellent, 0.3f },
				{ ItemRarity.Flawless, 0.1f }
			}
		},
		{
			LevelType.Bathrooms, new Dictionary<ItemRarity, float>()
			{
				{ ItemRarity.Broken, 0.2f },
				{ ItemRarity.Decrepit, 0.3f },
				{ ItemRarity.Worn, 0.4f },
				{ ItemRarity.Dusty, 0.6f },
				{ ItemRarity.Common, 0.9f },
				{ ItemRarity.Nice, 1.2f },
				{ ItemRarity.Great, 1.6f },
				{ ItemRarity.Excellent, 1.2f },
				{ ItemRarity.Flawless, 0.9f }
			}
		},
	};

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
