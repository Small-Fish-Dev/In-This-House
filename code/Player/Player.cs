namespace ITH;

public sealed partial class Player : Component
{
	[Property] public ContainerComponent Inventory { get; private set; }
	[Property] public PlayerController Controller { get; private set; }
	[Property] public SkinnedModelRenderer Model { get; private set; }
	[Sync] public int Money { get; private set; }
	public static Action<Player, int, int> OnMoneyChanged;

	protected override void OnStart()
	{
		Inventory ??= Components.Get<ContainerComponent>();
		Controller ??= Components.Get<PlayerController>();

		if ( IsProxy )
			return;

		Local.Player = this;
		SetMoney( 500 );
	}

	protected override void OnDestroy()
	{
		OnMoneyChanged = null;
	}

	protected override void OnFixedUpdate()
	{
		UpdateUse();
	}

	protected override void OnUpdate()
	{
	}

	protected override void OnPreRender()
	{
	}

	public void SetMoney( int value )
	{
		OnMoneyChanged?.Invoke( this, Money, value );
		Money = value;
	}

	// TODO:
	public bool HasUpgrade( string identifier ) => true;
}
