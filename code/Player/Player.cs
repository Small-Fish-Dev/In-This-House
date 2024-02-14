namespace ITH;

public sealed class Player : Component
{
	[Sync] public int Money { get; private set; }

	protected override void OnStart()
	{
		if ( IsProxy )
			return;

		Local.Player = this;
		Money = 500;
	}
}
