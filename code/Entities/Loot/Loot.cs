namespace BrickJam;

public partial class Loot : UsableEntity
{
	public LootPrefab Prefab;

	[Net] public LootRarity Rarity { get; set; } = LootRarity.Common;
	[Net] public int BaseMonetaryValue { get; set; } = 0;
	[Net] public string BaseName { get; set; } = "Loot";
	public new string Name => $"{Rarity} {Name}";
	public int MonetaryValue => (int)(BaseMonetaryValue * RarityMap[Rarity]);
	public static Dictionary<LootRarity, float> RarityMap { get; set; } = new()
	{
		{ LootRarity.Broken, 0.2f },
		{ LootRarity.Decrepit, 0.4f },
		{ LootRarity.Worn, 0.6f },
		{ LootRarity.Dusty, 0.8f },
		{ LootRarity.Common, 1f },
		{ LootRarity.Nice, 1.3f },
		{ LootRarity.Great, 1.8f },
		{ LootRarity.Excellent, 2.5f },
		{ LootRarity.Flawless, 4f }
	};
	public static Dictionary<LevelType, Dictionary<LootRarity, float>> RarityChances { get; set; } = new()
	{
		{
			LevelType.Mansion, new Dictionary<LootRarity, float>()
			{
				{ LootRarity.Broken, 1f },
				{ LootRarity.Decrepit, 1.2f },
				{ LootRarity.Worn, 1.4f },
				{ LootRarity.Dusty, 1.2f },
				{ LootRarity.Common, 1f },
				{ LootRarity.Nice, 0.6f },
				{ LootRarity.Great, 0.3f },
				{ LootRarity.Excellent, 0.1f },
				{ LootRarity.Flawless, 0.03f }
			}
		},
		{
			LevelType.Dungeon, new Dictionary<LootRarity, float>()
			{
				{ LootRarity.Broken, 0.5f },
				{ LootRarity.Decrepit, 0.7f },
				{ LootRarity.Worn, 0.9f },
				{ LootRarity.Dusty, 1f },
				{ LootRarity.Common, 1.2f },
				{ LootRarity.Nice, 1f },
				{ LootRarity.Great, 0.7f },
				{ LootRarity.Excellent, 0.3f },
				{ LootRarity.Flawless, 0.1f }
			}
		},
		{
			LevelType.Bathrooms, new Dictionary<LootRarity, float>()
			{
				{ LootRarity.Broken, 0.2f },
				{ LootRarity.Decrepit, 0.3f },
				{ LootRarity.Worn, 0.4f },
				{ LootRarity.Dusty, 0.6f },
				{ LootRarity.Common, 0.9f },
				{ LootRarity.Nice, 1.2f },
				{ LootRarity.Great, 1.6f },
				{ LootRarity.Excellent, 1.2f },
				{ LootRarity.Flawless, 0.9f }
			}
		},
	};

	public static Loot CreateFromGameResource( LootPrefab resource, Vector3 position, Rotation rotation )
	{
		var item = new Loot();
		item.Position = position;
		item.Rotation = rotation;
		item.BaseMonetaryValue = resource.MonetaryValue;
		item.BaseName = resource.Name;
		item.Rarity = RandomRarityFromLevel( MansionGame.Instance.CurrentLevel.Type );
		item.SetModel( resource.Model == string.Empty ? "models/error.vmdl" : resource.Model );
		item.SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		item.Prefab = resource;
		
		return item;
	}

	public static Loot CreateFromEntry( ItemEntry entry, Vector3 position, Rotation rotation )
	{
		var item = CreateFromGameResource( entry.Prefab, position, rotation );
		item.Tags.Add( "nocollide" );
		item.Rarity = entry.Rarity;
		return item;
	}

	public static LootRarity RandomRarityFromLevel( LevelType level ) => WeightedList.RandomKey<LootRarity>( RarityChances[level] );

	public override void Use( Player user )
	{
		if ( !IsAuthority )
			return;

		if ( user.Inventory.Add( new ItemEntry { Prefab = Prefab, Rarity = Rarity } ) )
			Delete();
	}
}
