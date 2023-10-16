namespace BrickJam;

public partial class Item : UseableEntity
{
	public ItemPrefab Prefab;

	[Net] public ItemRarity Rarity { get; set; } = ItemRarity.Common;
	[Net] public int BaseMonetaryValue { get; set; } = 0;
	[Net] public string BaseName { get; set; } = "Item";
	public new string Name => $"{Rarity} {Name}";
	public int MonetaryValue => (int)(BaseMonetaryValue * RarityMap[Rarity]);
	public static Dictionary<ItemRarity, float> RarityMap { get; set; } = new()
	{
		{ ItemRarity.Broken, 0.2f },
		{ ItemRarity.Decrepit, 0.4f },
		{ ItemRarity.Worn, 0.6f },
		{ ItemRarity.Dusty, 0.8f },
		{ ItemRarity.Common, 1f },
		{ ItemRarity.Nice, 1.3f },
		{ ItemRarity.Great, 1.8f },
		{ ItemRarity.Excellent, 2.5f },
		{ ItemRarity.Flawless, 4f }
	};

	public static Item CreateFromGameResource( ItemPrefab resource, Vector3 position, Rotation rotation )
	{
		var item = new Item();

		item.Tags.Add( "nocollide" );
		item.Position = position;
		item.Rotation = rotation;
		item.BaseMonetaryValue = resource.MonetaryValue;
		item.BaseName = resource.Name;
		item.SetModel( resource.Model == string.Empty ? "models/error.vmdl" : resource.Model );
		item.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );

		// TODO: Might not need to attach the ItemPrefab member at all. 
		// See if there's any reason to include it on the Item entity.
		item.Prefab = resource;
		
		return item;
	}

	public override void Use( Player user )
	{
		base.Use( user );

		user.AddMoney( MonetaryValue );
		Delete();
	}
}
