namespace BrickJam;

public abstract partial class GameState : Entity // Easy replication to client
{
	public GameState() => Transmit = TransmitType.Always;
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
}

public partial class Mansion
{
	[Net, Change] public GameState CurrentState { get; set; }

	public static async void SetState<T>() where T : GameState
	{
		if ( Instance.CurrentState != null )
		{
			await Instance.CurrentState?.End();
			Instance.CurrentState?.Delete();
		}

		Instance.CurrentState = Activator.CreateInstance<T>();
		await Instance.CurrentState.Start();
	}

	public void OnCurrentStateChanged()
	{
	}

	[GameEvent.Entity.PostSpawn]
	static void startStates()
	{
		SetState<PlayingState>();
	}
}
