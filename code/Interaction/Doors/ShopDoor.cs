namespace ITH;

public sealed class ShopDoor : Component
{
	[Property] private Usable _usable;

	protected override void OnStart()
	{
		_usable.OnUsed += Use;
		_usable.LockText = "lockpick the mansion door";
	}

	protected override void OnUpdate()
	{
		_usable.CanUse = true;
		_usable.UseString = _usable.CanUse ? "enter the mansion" : "ALL PLAYERS NEED TO BE NEARBY TO PROCEED";
	}

	private void Use( PlayerController user )
	{
		Log.Info( user );
		MansionGame.Instance.SetLevel( LevelType.Mansion );
	}
}
