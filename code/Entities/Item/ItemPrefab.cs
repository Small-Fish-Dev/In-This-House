namespace BrickJam;

// THE S&BOX PREFAB EDITOR BROKE LOLOLOL.
[GameResource( "ItemPrefab", "item", "Define item data.", Icon = "stars" )]
public class ItemPrefab : GameResource
{
	public string Name { get; set; }
	public string Description { get; set; }

	public ItemRarity Rarity { get; set; }

	// TODO: This could be a function of Rarity? 
	// Common => some_amount
	// Rare => higher_amount
	// Legendary => highest_amount
	public int MonetaryValue { get; set; }
}
