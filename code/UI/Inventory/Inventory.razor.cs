using BrickJam.Upgrading;
using Sandbox.UI;

namespace BrickJam.UI;

public partial class Inventory : Panel
{
	private Panel RefItemContainer { get; set; }

	private List<InventoryItem> Items { get; set; } = new();

	public Inventory()
	{
		instance = this;
	}

	[Event( "InventoryChanged" )]
	void OnInventoryChanged( IClient cl, ItemEntry entry, int count )
	{
		if ( Game.IsServer || cl != Game.LocalClient )
			return;

		var item = Items.FirstOrDefault( v => v.ItemEntry.Name == entry.Name );
		if ( item != null )
			return;

		// Create new item
		item = new InventoryItem( entry, count );
		item.AddClass( "item" );
		RefItemContainer.AddChild( item );
		Items.Add( item );
	}
}
