namespace ITH;

public struct ItemEntry : IEquatable<ItemEntry>
{
	public Loot Loot;
	public string Name => $"{Loot.Rarity} {Loot?.Name ?? "unknown"}";
	public int Price => (int)((Loot?.MonetaryValue ?? 0) * Loot.RarityMap[Loot.Rarity]);

	public bool Equals( ItemEntry other )
	{
		return other.Loot == Loot
			   && other.Loot.Rarity == Loot.Rarity;
	}

	public override bool Equals( object obj )
	{
		return obj is ItemEntry other
			   && Equals( other );
	}

	public override int GetHashCode()
	{
		return HashCode.Combine( Loot, Loot.Rarity );
	}
}

public sealed partial class ContainerComponent : Component
{
	/// <summary>
	/// Limited amount of space
	/// </summary>
	public int Limit { get; set; } = 20;

	/// <summary>
	/// Dictionary of all the items and amounts.
	/// </summary>
	public Dictionary<ItemEntry?, int> Loots { get; private set; }

	/// <summary>
	/// Adds some amount of items to the list.
	/// </summary>
	/// <param name="entry"></param>
	/// <param name="amount"></param>
	/// <returns>True if the operation was successful.</returns>
	public bool Add( ItemEntry entry, int amount = 1 )
	{
		var didAdd = false;

		if ( Loots.ContainsKey( entry ) )
		{
			Loots[entry] += amount;
			didAdd = true;
		}

		if ( Loots.Count < Limit && !didAdd )
		{
			Loots.Add( entry, amount );
			didAdd = true;
		}

		return didAdd;
	}

	/// <summary>
	/// Removes some amount of items from the list.
	/// </summary>
	/// <param name="entry"></param>
	/// <param name="amount"></param>
	/// <returns>True if the operation was successful.</returns>
	public bool Remove( ItemEntry entry, int amount = 1 )
	{
		if ( Has( entry ) < amount )
			return false;

		var didRemove = false;
		Loots[entry] -= amount;
		if ( Loots[entry] <= 0 )
		{
			Loots.Remove( entry );
			didRemove = true;
		}

		return didRemove;
	}

	/// <summary>
	/// Returns a number greater than 0 if the player has any amount of said item.
	/// </summary>
	/// <param name="item"></param>
	/// <returns></returns>
	public int Has( ItemEntry item )
	{
		if ( Loots.TryGetValue( item, out var amount ) )
			return amount;

		return 0;
	}

	public void Clear()
	{
		Loots.Clear();
	}
}
