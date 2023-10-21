namespace BrickJam;

public static class ClientExtension
{
	public static int GetSlot( this IClient client ) => client?.GetInt( "slot", -1 ) ?? -1;

	public static Color GetColor( this IClient client )
	{
		var slot = client.GetSlot();
		if ( slot < 0 )
			return Color.Gray;

		return MansionGame.SlotToColor[slot % MansionGame.SlotToColor.Length];
	}
}
