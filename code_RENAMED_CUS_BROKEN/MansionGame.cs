global using Sandbox;
global using System.Linq;
global using System.Threading.Tasks;
global using System.Collections.Generic;
global using System;
global using Sandbox.UI.Construct;
global using System.IO;
using BrickJam.UI;
using BrickJam.Upgrading;
using BrickJam.VoiceLines;
using Sandbox.Component;
using Sandbox.UI;

namespace BrickJam;

public partial class MansionGame : GameManager
{
	public static MansionGame Instance => (MansionGame)_instance?.Target;
	private static WeakReference _instance;

	[Net] public TimeUntil TimeOut { get; set; }
	[Net] public bool TimerActive { get; set; }

	public static int Seed { get; set; } = 0;
	public static Random Random { get; set; } = new Random();

	public float TimePerLevel => 180.0f;
	public float TimeToJoin => 5.0f;

	public MansionGame()
	{
	}

	public override void Spawn()
	{
		base.Spawn();

		_instance = new WeakReference( this );
		_ = new VoiceLinePlayer();

		ClientSlots = Enumerable.Repeat<IClient>( null, Game.Server.MaxPlayers ).ToList();

		ResetRandomSeed();
		Creator.Build();
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();
		_ = new VoiceLinePlayer();

		ClientSlots = Enumerable.Repeat<IClient>( null, Game.Server.MaxPlayers ).ToList();

		InitializeEffects();
		_ = new Hud();

		_instance = new WeakReference( this );
		Creator.Build();
	}

	public static void ResetRandomSeed()
	{
		if ( !Game.IsServer ) return;

		Seed = DateTime.UtcNow.Ticks.GetHashCode();
		Random = new Random( Seed );
	}

	public Transform GetSpawnPoint() => GetSpawnPoint( CurrentLevel.Type );

	public Transform GetSpawnPoint( LevelType level )
	{
		var spawnPoints = Entity.All.OfType<PlayerSpawn>()
			.Where( x => x.LevelType == level )
			.ToList();

		var randomSpawnPoint = MansionGame.Random.FromList( spawnPoints );

		if ( randomSpawnPoint == null )
			return new Transform( Vector3.Zero, Rotation.Identity, 1 );

		return randomSpawnPoint.Transform;
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		GiveSlot( client );

		if ( !TimerActive || TimePerLevel - TimeOut <= TimeToJoin )
		{
			var pawn = new Player();
			client.Pawn = pawn;
			pawn.Respawn();
		}
		else
		{
			var pawn = new Spectator();
			client.Pawn = pawn;
		}
	}

	public override void ClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );

		ReleaseSlot( cl );

		if ( cl.Pawn is Player player )
			player.Delete();
		else if ( cl.Pawn is Spectator spectator )
		{
			var p = spectator.Body;
			if ( p.IsValid() )
				p.Delete();
			spectator.Delete();
		}
	}

	const float SAVE_TIME = 15f;
	TimeSince lastSaved;

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		if ( Game.IsClient && lastSaved >= SAVE_TIME )
		{
			Player.StoreSave();
			lastSaved = 0f;
		}

		if ( !IsAuthority )
			return;

		SimulateTimer();
	}

	public void TimerStart()
	{
		TimerActive = true;
		TimeOut = TimePerLevel;
	}

	public void TimerStop()
	{
		TimerActive = false;
		TimeOut = 0;
	}

	private void SimulateTimer()
	{
		if ( TimerActive && TimeOut )
		{
			TimerStop();
			RestartGame();
		}
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		BugBug.Here( v =>
		{
			v.Text( "small fish jam game" );
			v.Value( "time", DateTime.Now );
			v.Space();
			v.Value( "music volume", CurrentMusicVolume );
			v.Space();

			v.Group( "local camera", () =>
			{
				v.Value( "pos", Camera.Position );
				v.Value( "ang", Camera.Rotation.Angles() );
			} );
		} );

		foreach ( var player in All.Where( ent => ent is Player player && player.IsValid() ) )
		{
			var glow = player.Components.GetOrCreate<Glow>();
			glow.Color = player.Client.GetColor();
			glow.Width = 0.5f;
			glow.Enabled = Game.LocalPawn is Spectator spec && spec.Following != player;
		}
	}

	public override void RenderHud()
	{
		base.RenderHud();
		Event.Run( "render" );
	}


	[GameEvent.Entity.PostSpawn]
	static async void checkForLockedDoorLMFAO()
	{
		await GameTask.Delay( 200 );

		var allShopDoors = Entity.All.OfType<ShopDoor>();
		
		if ( allShopDoors.Count() == 0 )
		{
			Log.Error( "ISSUE #4105 ENCOUNTERED [RESTART S&BOX IF THIS PERSISTS]" );
			Game.ChangeLevel( "mansion" );
			return;
		}

		await GameTask.Delay( 200 );

		var firstShopDoor = allShopDoors.FirstOrDefault();

		if ( firstShopDoor == null || !firstShopDoor.IsValid() )
		{
			Log.Error( "ISSUE #4105 ENCOUNTERED [RESTART S&BOX IF THIS PERSISTS]" );
			Game.ChangeLevel( "mansion" );
			return;
		}

		await GameTask.Delay( 200 );

		var lockComponent = firstShopDoor.Components.TryGet<LockedComponent>( out LockedComponent component );

		if ( !lockComponent )
		{
			Log.Error( "ISSUE #4105 ENCOUNTERED [RESTART S&BOX IF THIS PERSISTS]" );
			Game.ChangeLevel( "mansion" );
			return;
		}
	}
}
