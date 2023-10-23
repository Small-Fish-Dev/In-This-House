namespace BrickJam;

public struct ItemEntry : IEquatable<ItemEntry>
{
	public LootPrefab Prefab;
	public LootRarity Rarity;

	public string Name => $"{Rarity} {Prefab?.Name ?? "unknown"}";
	public int Price => (int)((Prefab?.MonetaryValue ?? 0) * Loot.RarityMap[Rarity]);

	public bool Equals( ItemEntry other )
	{
		return other.Prefab == Prefab
		       && other.Rarity == Rarity;
	}

	public override bool Equals( object obj )
	{
		return obj is ItemEntry other
		       && Equals( other );
	}

	public override int GetHashCode()
	{
		return HashCode.Combine( Prefab, Rarity );
	}
}

public partial class ContainerComponent : EntityComponent
{
	/// <summary>
	/// Limited amount of space
	/// </summary>
	[Net]
	public int Limit { get; set; } = 20;

	/// <summary>
	/// Dictionary of all the items and amounts.
	/// </summary>
	public IReadOnlyDictionary<ItemEntry?, int> Loots => items;

	private static Dictionary<ItemEntry?, int> clItems = new();
	private Dictionary<ItemEntry?, int> shItems = new();
	private Dictionary<ItemEntry?, int> items => Game.IsClient ? clItems : shItems;

	private IClient client => Entity.Client;

	/// <summary>
	/// Adds some amount of items to the list.
	/// </summary>
	/// <param name="entry"></param>
	/// <param name="amount"></param>
	/// <returns>True if the operation was successful.</returns>
	public bool Add( ItemEntry entry, int amount = 1 )
	{
		Game.AssertServer();
		var success = false;

		if ( items.ContainsKey( entry ) )
		{
			items[entry] += amount;
			success = true;
		}

		if ( Loots.Count < Limit && !success )
		{
			items.Add( entry, amount );
			success = true;
		}

		if ( success )
		{
			if ( client != null )
				sendUpdate( To.Single( client ), entry.Prefab.ResourceName, entry.Rarity, items[entry] );

			Event.Run( "InventoryChanged", client );
		}

		return success;
	}

	/// <summary>
	/// Removes some amount of items from the list.
	/// </summary>
	/// <param name="entry"></param>
	/// <param name="amount"></param>
	/// <returns>True if the operation was successful.</returns>
	public bool Remove( ItemEntry entry, int amount = 1 )
	{
		Game.AssertServer();

		if ( Has( entry ) < amount )
			return false;

		var delete = false;
		items[entry] -= amount;
		if ( items[entry] <= 0 )
		{
			items.Remove( entry );
			delete = true;
		}

		if ( client != null )
			sendUpdate( To.Single( client ), entry.Prefab.ResourceName, entry.Rarity, delete ? 0 : items[entry] );

		Event.Run( "InventoryChanged", client );

		return true;
	}

	/// <summary>
	/// Returns a number greater than 0 if the player has any amount of said item.
	/// </summary>
	/// <param name="item"></param>
	/// <returns></returns>
	public int Has( ItemEntry item )
	{
		var amount = 0;
		_ = items.TryGetValue( item, out amount );

		return amount;
	}

	public void Clear()
	{
		forceReloadItemInfo( To.Single( Entity ) );
		Game.AssertServer();
		items.Clear();
	}

	[ClientRpc]
	private void forceReloadItemInfo()
	{
		try
		{
			Log.Warning( "FORCE RELOADING INVENTORY!" );
			Event.Run( "InventoryChanged", client );
		}
		catch ( Exception e )
		{
			Log.Warning( "Failed to force reload inventory!" );
			Log.Warning( e );
		}
	}

	[ClientRpc]
	private void sendUpdate( string prefabName, LootRarity rarity, int amount )
	{
		var prefab = LootPrefab.Get( prefabName );
		if ( prefab == null )
		{
			Log.Warning( $"Couldn't find the item {prefabName}." );
			return;
		}

		var delete = false;
		var entry = new ItemEntry { Prefab = prefab, Rarity = rarity };

		items[entry] = amount;
		if ( items[entry] <= 0 )
		{
			items.Remove( entry );
			delete = true;
		}

		Event.Run( "InventoryChanged", client );

		// HACK: just save
		(Game.LocalPawn as Player)?.StoreSave();
	}
}
