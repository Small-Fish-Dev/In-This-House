namespace BrickJam;

public partial class MansionGame : GameManager
{
	[ConCmd.Server( "give_item" )]
	public static void GiveItem( string name, int amount = 1 )
	{
		if ( ConsoleSystem.Caller.Pawn is not Player pawn )
			return;

		var item = ItemPrefab.Get( name );
		if ( item == null )
			return;

		pawn.Inventory.Add( item, amount );
	}

	[ConCmd.Admin( "set_state" )]
	public static void SetState( string state )
	{
		if ( state == "play" || state == "playing" )
			SetState<PlayingState>();
	}

	[ConCmd.Admin( "set_level" )]
	public static void SetLevel( string level )
	{
		if ( level == "mansion" )
			SetLevel<MansionLevel>();
	}

	[ConCmd.Server( "mansion_spectate" )]
	public static void Spectate()
	{
		if ( ConsoleSystem.Caller is null )
			return;

		if ( ConsoleSystem.Caller.Pawn is Player player )
		{
			Log.Error( "TODO: kill the player instead" );
			player.Delete();
		}

		var spectator = new Spectator();
		ConsoleSystem.Caller.Pawn = spectator;
	}
}
