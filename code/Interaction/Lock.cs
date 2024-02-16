namespace ITH;

public sealed class Lock : Component
{
	[Sync] public bool Locked { get; set; } = true;

	public void Lockpick( Player target )
	{
		if ( !Locked )
			return;
	}

	public void Unlock()
	{
		Locked = false;
	}

	public void RequestLockpicked()
	=> Log.Info( "" );

	public void RequestLockpicked( Guid ident )
	{
		var entity = Scene.Directory.FindByGuid( ident );
		if ( entity == null )
			return;

		var component = entity.Components.Get<Lock>();
		if ( component == null )
			return;

		component.Unlock();
	}

	protected override void OnDisabled()
	{
		// Lock?.Delete();
	}

	[Broadcast]
	public void LockPick( Guid ident )
	{
		var go = Scene.Directory.FindByGuid( ident );
		if ( go == null )
			return;

		var component = go.Components.Get<Lock>();
		if ( component == null )
			return;

		if ( !ITH.UI.Lockpick.Active )
			ITH.UI.Lockpick.Open( component );
	}
}
