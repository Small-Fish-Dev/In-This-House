using BrickJam.Upgrading;
using Sandbox.UI;

namespace BrickJam.UI;

public partial class Inventory : Panel
{
	private List<InventoryItem> Items { get; set; } = new();
	
	public Inventory()
	{
		instance = this;
	}
}
