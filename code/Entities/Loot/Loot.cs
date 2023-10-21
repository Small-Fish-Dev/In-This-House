using BrickJam.UI;

namespace BrickJam;

public partial class Loot : UsableEntity
{
	public LootPrefab Prefab;
	
	public override string UseString => $"take the {FullName}";

	public override float InteractionDuration => 0;
	[Net] public LootRarity Rarity { get; set; } = LootRarity.Common;
	[Net] public int BaseMonetaryValue { get; set; } = 0;
	[Net] public string BaseName { get; set; } = "Loot";
	public string FullName => $"{Rarity} {BaseName}";
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

	public static Loot CreateFromGameResource( LootPrefab resource, Vector3 position, Rotation rotation, bool setRarity = true )
	{
		var loot = new Loot();
		loot.Position = position;
		loot.Rotation = rotation;
		loot.BaseMonetaryValue = resource.MonetaryValue;
		loot.BaseName = resource.Name;
		if ( setRarity )
			loot.Rarity = RandomRarityFromLevel( MansionGame.Instance.CurrentLevel.Type );
		loot.SetModel( resource.Model == string.Empty ? "models/error.vmdl" : resource.Model );
		loot.SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		loot.Tags.Add( "loot" );

		loot.Prefab = resource;
		
		return loot;
	}

	public static Loot CreateFromEntry( ItemEntry entry, Vector3 position, Rotation rotation )
	{
		var loot = CreateFromGameResource( entry.Prefab, position, rotation, false );
		loot.Rarity = entry.Rarity;
		return loot;
	}

	public static LootRarity RandomRarityFromLevel( LevelType level ) => WeightedList.RandomKey<LootRarity>( RarityChances[level] );

	Player picker = null;
	bool deleting = false;

	public override void Use( Player user )
	{
		if ( !IsAuthority || picker != null )
			return;

		picker = user;
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic );

		var normal = (user.Position - Position).Normal.WithZ( 1f );
		var force = 100f + Position.Distance( user.Position );
		ApplyAbsoluteImpulse( force * normal );
	}

	[GameEvent.Tick.Server]
	private void effect()
	{
		if ( picker == null )
		{
			Scale = MathX.Lerp( Scale, 1f, 5f * Time.Delta );
			return;
		}

		Scale = MathX.Lerp( Scale, 0, 5f * Time.Delta );
		if ( Scale.AlmostEqual( 0, 0.1f ) && !deleting )
		{
			picker.Inventory.Add( new ItemEntry { Prefab = Prefab, Rarity = Rarity } );
			Delete();
		}
	}

	private GroundLootPanel panel;
	public override void ClientSpawn()
	{
		base.ClientSpawn();
		panel = new GroundLootPanel
		{
			Loot = this
		};
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		panel?.Delete();
	}
}
