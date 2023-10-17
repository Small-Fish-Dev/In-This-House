namespace BrickJam;

public partial class MansionGame : GameManager
{
	[ConCmd.Server( "give_item" )]
	public static void GiveLoot( string name, LootRarity rarity = LootRarity.Excellent, int amount = 1 )
	{
		if ( ConsoleSystem.Caller.Pawn is not Player pawn )
			return;

		var item = LootPrefab.Get( name );
		if ( item == null )
			return;

		pawn.Inventory.Add( new ItemEntry { Prefab = item, Rarity = rarity }, amount );
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
		if ( ConsoleSystem.Caller is null || ConsoleSystem.Caller.Pawn is Spectator )
			return;

		var t = Instance.GetSpawnPoint();
		if ( ConsoleSystem.Caller.Pawn is Player player )
		{
			Log.Error( "TODO: kill the player instead" );
			t = new Transform(player.EyePosition, player.Rotation);
			player.Delete();
		}

		var spectator = new Spectator { Transform = t };
		ConsoleSystem.Caller.Pawn = spectator;
	}

	[ConCmd.Admin( "mansion_timer_stop" )]
	public static void ConTimerStop()
	{
		Instance.TimerStop();
	}
}
