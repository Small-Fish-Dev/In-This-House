namespace BrickJam;

public partial class ContainerComponent : EntityComponent
{
	/// <summary>
	/// Dictionary of all the items and amounts.
	/// </summary>
	public IReadOnlyDictionary<ItemPrefab, int> Items => items;
	private Dictionary<ItemPrefab, int> items = new();
	private IClient client => Entity.Client;

	/// <summary>
	/// Adds some amount of items to the list.
	/// </summary>
	/// <param name="item"></param>
	/// <param name="amount"></param>
	public void Add( ItemPrefab item, int amount = 1 )
	{
		Game.AssertServer();

		if ( items.ContainsKey( item ) )
		{
			items[item] += amount;
			return;
		}

		items.Add( item, amount );

		if ( client != null )
			sendUpdate( To.Single( client ), item.ResourceName, items[item] );
	}

	/// <summary>
	/// Removes some amount of items from the list.
	/// </summary>
	/// <param name="item"></param>
	/// <param name="amount"></param>
	/// <returns>True if the operation was successful.</returns>
	public bool Remove( ItemPrefab item, int amount = 1 )
	{
		Game.AssertServer();

		if ( Has( item ) < amount )
			return false;

		items[item] -= amount;
		if ( items[item] <= 0 )
			items.Remove( item );

		if ( client != null )
			sendUpdate( To.Single( client ), item.ResourceName, items[item] );

		return true;
	}

	/// <summary>
	/// Returns a number greater than 0 if the player has any amount of said item.
	/// </summary>
	/// <param name="item"></param>
	/// <returns></returns>
	public int Has( ItemPrefab item )
	{
		var amount = 0;
		_ = items.TryGetValue( item, out amount );

		return amount;
	}

	[ClientRpc]
	private void sendUpdate( string prefabName, int amount )
	{
		var prefab = ItemPrefab.Get( prefabName );
		if ( prefab == null )
		{
			Log.Warning( $"Couldn't find the item {prefabName}." );
			return;
		}

		items[prefab] = amount;
		if ( items[prefab] <= 0 )
			items.Remove( prefab );
	}
}
