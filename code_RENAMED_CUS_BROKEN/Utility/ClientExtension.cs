namespace BrickJam;

public static class ClientExtension
{
	public static int GetSlot( this IClient client )
	{
		var slot = client?.GetInt( "slot", -1 ) ?? -1;
		if ( slot >= Game.Server.MaxPlayers )
		{
			Log.Error( "S&BOX MOMENT!!!" );
			return -1;
		}

		return slot;
	}

	public static Color GetColor( this IClient client )
	{
		var slot = client.GetSlot();
		if ( slot < 0 )
			return Color.Gray;

		return MansionGame.SlotToColor[slot % MansionGame.SlotToColor.Length];
	}
}
