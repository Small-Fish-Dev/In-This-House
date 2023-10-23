using BrickJam.Upgrading;
using Sandbox.UI;

namespace BrickJam.UI;

public partial class InventoryItem : Panel
{
	private ItemEntry ItemEntry { get; set; }
	private int Count { get; set; }

	public InventoryItem( ItemEntry entry )
	{
		ItemEntry = entry;

		Style.SetBackgroundImage( ItemEntry.Prefab.Icon );
	}

	[Event( "InventoryChanged" )]
	void OnInventoryChanged( IClient cl, ItemEntry entry, int count )
	{
		if ( Game.IsServer || cl != Game.LocalClient )
			return;

		if ( ItemEntry.Name != entry.Name )
			return;

		Count = count;

		if ( Count == 0 )
		{
			Delete();
			return;
		}

		Log.Info( $"updated item panel for {entry.Name}, new count {Count}" );

		StateHasChanged();
	}
}
