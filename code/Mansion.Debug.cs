namespace BrickJam;

public partial class MansionGame : GameManager
{
	[ConCmd.Admin( "give_item" )]
	public static void GiveLoot( string name, LootRarity rarity = LootRarity.Excellent, int amount = 1 )
	{
		if ( ConsoleSystem.Caller.Pawn is not Player pawn )
			return;

		var item = LootPrefab.Get( name );
		if ( item == null )
			return;

		pawn.Inventory.Add( new ItemEntry { Prefab = item, Rarity = rarity }, amount );
	}

	[ConCmd.Admin( "set_money" )]
	public static void SetMoney( int amount )
	{
		if ( ConsoleSystem.Caller.Pawn is not Player pawn )
			return;

		pawn.SetMoney( amount );
	}

	[ConCmd.Admin( "set_level" )]
	public static void SetLevel( string level )
	{
		if ( level == "mansion" )
			SetLevel<MansionLevel>();
	}

	[ConCmd.Server( "mansion_die" )]
	public static void Die()
	{
		if ( ConsoleSystem.Caller is null || ConsoleSystem.Caller.Pawn is not Player player )
			return;

		player.Kill();
	}

	[ConCmd.Admin( "mansion_timer_stop" )]
	public static void ConTimerStop()
	{
		Instance.TimerStop();
	}
}
