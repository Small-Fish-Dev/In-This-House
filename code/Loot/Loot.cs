namespace ITH;

public enum LootRarity
{
	Broken,
	Decrepit,
	Worn,
	Dusty,
	Common,
	Nice,
	Great,
	Excellent,
	Flawless
}

[Icon( "currency_exchange" )]
public partial class Loot : Component
{
	[Property] public string Name { get; private set; }
	[Property] public string Description { get; private set; }
	[Property] public LootRarity Rarity { get; set; }
	[Property] public LevelType LevelCanAppearOn { get; private set; }
	[Property] public int MonetaryValue { get; private set; }
	[Property] public bool DisplayFront { get; private set; }
}
