namespace BrickJam;

public partial class Item : UseableEntity
{
	public ItemPrefab Prefab;

	[Net]
	public ItemRarity Rarity { get; set; }

	[Net]
	public int MonetaryValue { get; set; }

	public static Item CreateFromGameResource( ItemPrefab resource , Vector2 position, Rotation rotation)
	{
		var item = new Item();

		item.Position = position;
		item.Rotation = rotation;
		item.Rarity = resource.Rarity;
		item.MonetaryValue = resource.MonetaryValue;

		// TODO: Might not need to attach the ItemPrefab member at all. 
		// See if there's any reason to include it on the Item entity.
		item.Prefab = resource;
		
		return item;
	}
}
