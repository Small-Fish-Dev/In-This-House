namespace ITH;

partial class Loot
{
	// TODO: Move this into data!!!??
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
}