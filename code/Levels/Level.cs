﻿using BrickJam.UI;
using Sandbox;
using System.Diagnostics.Tracing;
using static Sandbox.Gizmo;

namespace BrickJam;

public enum LevelType // We need this to categorize hammer entities
{
	None,
	Shop,
	Mansion,
	Dungeon,
	Bathrooms
}

public abstract partial class Level : Entity // Easy replication to client
{
	public virtual LevelType Type { get; set; } = LevelType.None;
	public abstract string Music { get; }
	[Net] public UsableEntity Exit { get; set; } = null;
	[Net] public IList<NPC> Monsters { get; set; }
	[Net] public TimeSince SinceStarted { get; set; } = 0f;

	public Level() => Transmit = TransmitType.Always;

	public static bool GameIsEnding { get; set; } = false;

	[GameEvent.Tick.Server]
	public virtual void Compute() // TODO: When restarting might be a problem if it runs while it awaits Start and End
	{
		if ( All.OfType<Player>().Any() && All.OfType<Player>().All( x => !x.IsAlive ) && !GameIsEnding )
		{
			MansionGame.RestartGame();
			GameIsEnding = true;
			Eventlog.Send( "<red>Looks like everyone died, better luck next time!", To.Everyone );
		}
	}

	protected void RespawnAll()
	{
		foreach ( var client in Game.Clients )
		{
			switch ( client.Pawn )
			{
				case Player player:
					player.Respawn();
					break;

				case Spectator spectator:
					{
						var pawn = spectator.Body.IsValid() ? spectator.Body : new Player();
						client.Pawn = pawn;
						pawn.Respawn();
						spectator.Delete();
						break;
					}
			}
		}
	}

	public virtual async Task Start()
	{
		await GameTask.NextPhysicsFrame();

		RespawnAll();

		foreach ( var player in Entity.All.OfType<Player>().Where( p => p.Client is null ) )
			player.Delete();

		foreach ( var player in Entity.All.OfType<Player>().ToList() )
		{
			if ( player.HasUpgrade( "Cartoony Sidekick" ) )
			{
				var doob = new Doob( this );
				doob.Position = player.Position;
				doob.Rotation = player.Rotation;
				doob.Owner = player;
				player.Doob = doob;
			}
		}

		Exit?.Delete(); // Just make sure

		if ( Type == LevelType.Bathrooms )
		{
			var allValidFinalDoors = Entity.All.OfType<ValidFinalDoorPosition>()
				.ToList();
			var randomFinalDoor = MansionGame.Random.FromList( allValidFinalDoors );

			Exit = new FinalDoor();
			Exit.Position = randomFinalDoor.Position;
		}
		else
		{
			var allValidTrapdoors = Entity.All.OfType<ValidTrapdoorPosition>()
				.Where( x => x.LevelType == Type )
				.ToList();
			var randomValidTrapdoor = MansionGame.Random.FromList( allValidTrapdoors );

			Exit = new Trapdoor();
			Exit.Position = randomValidTrapdoor.Position;
		}

		var spawners = Entity.All.OfType<LootSpawner>()
			.Where( x => x.LevelType == Type )
			.ToList();

		for ( int i = 0; i < spawners.Count; i++ )
			spawners[i]?.SpawnLoot();

		var allDoorsInLevel = Entity.All.OfType<Door>()
			.Where( x => x.LevelType == Type )
			.ToList();

		for ( int i = 0; i < allDoorsInLevel.Count; i++ )
			allDoorsInLevel[i]?.Close();

		await GameTask.Delay( 1000 ); // Wait for any door to close lol :-)

		await GenerateGrid();

		GameIsEnding = false;

		MansionGame.Instance.TimerStart();

		return;
	}

	public virtual async Task End()
	{
		await GameTask.NextPhysicsFrame();

		BlackScreen.Start( To.Everyone, 2f, 1f, 1f );
		await GameTask.DelaySeconds( 1f ); // Wait for the black screen to be fully black

		Exit?.Delete();

		foreach ( var monster in new List<NPC>( Monsters ) )
			RemoveMonster( monster );

		foreach ( var spawner in Entity.All.OfType<LootSpawner>() )
			spawner.DeleteLoot();

		var allDoorsInLevel = Entity.All.OfType<Door>()
			.Where( x => WorldBox.Contains( x.Position ) ); // Only doors inside of this level

		foreach ( var door in allDoorsInLevel )
			door.Close();

		foreach ( var loot in Entity.All.OfType<Loot>() )
			loot.Delete();

		MansionGame.Instance.TimerStop();
		
		foreach ( var doob in Entity.All.OfType<Doob>() )
		{
			doob.Owner.Doob = null;
			doob.Delete();
		}

		return;
	}

	public virtual void RegisterMonster( NPC monster ) => Monsters.Add( monster );

	public virtual void RemoveMonster( NPC monster )
	{
		Monsters.Remove( monster );
		monster.Delete();
	}

	public static Type GetType( LevelType type )
	{
		return type switch
		{
			LevelType.Shop => typeof(ShopLevel),
			LevelType.Mansion => typeof(MansionLevel),
			LevelType.Dungeon => typeof(DungeonLevel),
			LevelType.Bathrooms => typeof(BathroomsLevel),
			_ => null,
		};
	}

	public static Type GetNextType( LevelType type ) => GetType( type + 1 );
	public static Type GetPreviousType( LevelType type ) => GetType( type - 1 );
}

public partial class MansionGame
{
	[Net, Change] public Level CurrentLevel { get; set; }

	public float MusicVolumeChangeRate => 0.5f;

	[Net] public Sound CurrentMusic { get; set; }
	[Net] public float CurrentMusicVolume { get; set; } = 0;
	[Net] public float TargetMusicVolume { get; set; } = 0;
	[Net] public TimeSince MusicChange { get; set; } = 0;

	[GameEvent.Tick.Server]
	protected void ProcessMusic()
	{
		Game.AssertServer();
		
		// No level or the music is mute
		if ( CurrentLevel is null
		     || CurrentLevel.Music?.Length == 0
		     || CurrentMusicVolume.AlmostEqual( 0, 0.001f ) && TargetMusicVolume == 0 )
		{
			if ( CurrentMusic.IsPlaying )
				CurrentMusic.Stop();

			return;
		}

		if ( !CurrentMusic.IsPlaying && MusicChange > 1 )
		{
			CurrentMusic = Sound.FromScreen( CurrentLevel.Music );
			MusicChange = 0;
		}

		CurrentMusicVolume = CurrentMusicVolume.LerpTo( TargetMusicVolume, MusicVolumeChangeRate );
		CurrentMusic.SetVolume( CurrentMusicVolume );
	}

	public static async void SetLevel<T>() where T : Level
	{
		if ( !Game.IsServer ) return;

		Instance.TargetMusicVolume = 0;

		if ( Instance.CurrentLevel != null )
		{
			await Instance.CurrentLevel?.End();
			Instance.CurrentLevel?.Delete();
		}

		Instance.CurrentLevel = Activator.CreateInstance<T>();
		await Instance.CurrentLevel.Start();
		Instance.TargetMusicVolume = 1;
	}

	public static void NextLevel()
	{
		if ( Instance.CurrentLevel is ShopLevel )
			SetLevel<MansionLevel>();
		else if ( Instance.CurrentLevel is MansionLevel )
			SetLevel<DungeonLevel>();
		else if ( Instance.CurrentLevel is DungeonLevel )
			SetLevel<BathroomsLevel>();
		else if ( Instance.CurrentLevel is BathroomsLevel )
			SetLevel<ShopLevel>();
	}

	// rndtrash: oh my god i hate it so much // ubre: fixed it for you
	public async static void RestartLevel()
	{
		await Instance.CurrentLevel?.End();
		await Instance.CurrentLevel?.Start();
	}

	public static void RestartGame()
	{
		ResetRandomSeed();
		SetLevel<ShopLevel>();
	}

	// For client callback
	public void OnCurrentLevelChanged()
	{
		Player.StoreSave();
	}

	[GameEvent.Entity.PostSpawn]
	static void startLevels()
	{
		RestartGame();
	}
}
