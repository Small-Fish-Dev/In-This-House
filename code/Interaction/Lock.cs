namespace ITH;

public sealed class Lock : Component
{
	[Sync] public bool Locked { get; set; } = true;

	public void Lockpick( PlayerController target )
	{
		if ( !Locked )
			return;
	}

	public void Unlock()
	{
		Locked = false;
	}
}
