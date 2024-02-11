namespace BrickJam;

public partial class MansionGame
{
	public static readonly Color[] SlotToColor =
	{
		Color.Red, Color.Green, Color.Blue, Color.Yellow
	};
	
	[Net] public IList<IClient> ClientSlots { get; set; }

	protected void GiveSlot( IClient client )
	{
		var firstEmptySlot = ClientSlots.IndexOf( null );
		if ( firstEmptySlot == -1 )
			throw new Exception( "Cannot find an empty slot, this should never happen" );
		ClientSlots[firstEmptySlot] = client;
		client.SetInt( "slot", firstEmptySlot );
	}

	protected void ReleaseSlot( IClient client )
	{
		var clientSlot = ClientSlots.IndexOf( client );
		if ( clientSlot != -1 )
		{
			ClientSlots[clientSlot] = null;
			client.SetInt( "slot", -1 );
		}
	} 
}
