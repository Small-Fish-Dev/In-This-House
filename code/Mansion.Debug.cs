namespace BrickJam;

public partial class MansionGame : GameManager
{
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
