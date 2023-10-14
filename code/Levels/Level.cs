namespace BrickJam;

public enum LevelType // We need this to categorize hammer entities
{
	None,
	Mansion,
	Dungeon,
	Office,
	Library
}

public abstract partial class Level : Entity // Easy replication to client
{
	public Level() => Transmit = TransmitType.Always;
	public virtual LevelType Type { get; set; } = LevelType.None;
	[Net] public Trapdoor Trapdoor { get; set; } = null;
	[Net] public TimeSince SinceStarted { get; set; } = 0f;

	[GameEvent.Tick.Server]
	public virtual void Compute() { }

	public virtual async Task Start()
	{
		await GameTask.NextPhysicsFrame();
		return;
	}
	public virtual async Task End()
	{
		await GameTask.NextPhysicsFrame();
		return;
	}

	public static Type GetType( LevelType type )
	{
		switch ( type )
		{
			case LevelType.None:
				return null;
			case LevelType.Mansion:
				return typeof( MansionLevel );
			case LevelType.Dungeon:
				return typeof( DungeonLevel );
			case LevelType.Office:
				return typeof( OfficeLevel );
			case LevelType.Library:
				return typeof( LibraryLevel );
			default:
				return null;
		}
	}

	public static Type GetNextType( LevelType type ) => GetType( type + 1 );
	public static Type GetPreviousType( LevelType type ) => GetType( type - 1 );
}

public partial class MansionGame
{
	[Net, Change] public Level CurrentLevel { get; set; }

	public static async void SetLevel<T>() where T : Level
	{
		if ( !Game.IsServer ) return;

		if ( Instance.CurrentLevel != null )
		{
			await Instance.CurrentLevel?.End();
			Instance.CurrentLevel?.Delete();
		}

		Instance.CurrentLevel = Activator.CreateInstance<T>();
		await Instance.CurrentLevel.Start();
	}

	public static void NextLevel()
	{
		if ( Instance.CurrentLevel is MansionLevel )
			SetLevel<DungeonLevel>();
		else if ( Instance.CurrentLevel is DungeonLevel )
			SetLevel<OfficeLevel>();
		else if ( Instance.CurrentLevel is OfficeLevel )
			SetLevel<LibraryLevel>();
	}

	// For client callback
	public void OnCurrentLevelChanged()
	{
	}

	[GameEvent.Entity.PostSpawn]
	static void startLevels()
	{
		SetLevel<MansionLevel>();
	}
}
