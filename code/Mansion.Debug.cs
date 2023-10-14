namespace BrickJam;

public partial class Mansion : GameManager
{
	[ConCmd.Admin( "set_state" )]
	public static void SetState( string state )
	{
		if ( state == "play" || state == "playing" )
			SetState<PlayingState>();
	}
}
