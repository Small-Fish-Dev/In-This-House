using BrickJam.UI;

namespace BrickJam;

partial class Player
{
	public struct ItemSave
	{
		public string PrefabPath { get; set; }
		public LootRarity Rarity { get; set; }
		public int Count { get; set; }

		public static ItemSave Load( string data ) => Json.Deserialize<ItemSave>( data );

		public static ItemSave FromItemEntry( ItemEntry entry, int count )
		{
			var x = new ItemSave { PrefabPath = entry.Prefab.ResourceName, Rarity = entry.Rarity, Count = count };
			return x;
		}
	}

	public struct PlayerSave
	{
		public int Money { get; set; }
		public List<ItemSave> Inventory { get; set; }
		public List<string> Upgrades { get; set; }

		public static PlayerSave Load( string data ) => Json.Deserialize<PlayerSave>( data );

		public static PlayerSave? LoadStored()
		{
			Game.AssertClient();
			var z = $"{Game.LocalClient.SteamId}.player";
			if ( !FileSystem.Data.FileExists( z ) )
				return null;
			return Load( FileSystem.Data.ReadAllText( z ) );
		}

		public void SaveStored()
		{
			Game.AssertClient();
			FileSystem.Data.WriteJson( $"{Game.LocalClient.SteamId}.player", this );
		}

		public static PlayerSave FromPlayer( Player player )
		{
			var x = new PlayerSave
			{
				Inventory = new List<ItemSave>(), Upgrades = new List<string>(), Money = player.Money
			};

			foreach ( var (key, count) in player.Inventory.Loots )
			{
				if ( key == null )
				{
					Log.Warning( "item entry was null???" );
					continue;
				}

				x.Inventory.Add( ItemSave.FromItemEntry( key.Value, count ) );
			}

			foreach ( var key in player.Upgrades ) x.Upgrades.Add( key );

			return x;
		}
	}

	public void LoadSaveData( PlayerSave save )
	{
		Game.AssertServer();

		Inventory.Clear();
		Upgrades.Clear();
		Money = 0;

		foreach ( var saveItem in save.Inventory )
		{
			var prefab = LootPrefab.Get( saveItem.PrefabPath );
			if ( prefab == null )
			{
				Log.Warning( $"no item prefab for {saveItem.PrefabPath}" );
				continue;
			}

			Inventory.Add( new ItemEntry() { Prefab = prefab, Rarity = saveItem.Rarity }, saveItem.Count );
		}

		foreach ( var saveUpgrade in save.Upgrades )
		{
			Upgrades.Add( saveUpgrade );
		}

		Money = save.Money;
	}

	public void StoreSave()
	{
		Log.Info( "Saving client data" );
		PlayerSave.FromPlayer( this ).SaveStored();
	}

	private void SendSaveToServer( PlayerSave save )
	{
		Game.AssertClient();
		var json = Json.Serialize( save );
		ReceivePlayerSaveFromClient( json );
	}

	[ConCmd.Server( "ith_send_save" )]
	private static void ReceivePlayerSaveFromClient( string json )
	{
		try
		{
			var save = Json.Deserialize<PlayerSave>( json );
			if ( ConsoleSystem.Caller.Pawn is not Player player )
			{
				Log.Warning( $"{ConsoleSystem.Caller.Name} doesn't have a player! Not loading their save!" );
				return;
			}

			player.LoadSaveData( save );
		}
		catch ( Exception e )
		{
			Log.Warning( $"{ConsoleSystem.Caller.Name} sent invalid save data" );
		}
	}
}
