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
		if ( Input.Released( GameInputActions.Ping ) )
		{

		}
	}

	protected override void OnPreRender()
	{
	}

	public void SetMoney( int value )
	{
		OnMoneyChanged?.Invoke( this, Money, value );
		Money = value;
	}

	private bool Drop( ItemEntry? item )
	{
		if ( !item.HasValue )
			return false;

		// TODO: this is fucky im sorry

		var toDrop = Inventory.Loots.First( x => x.Key.HasValue && x.Key.Value.Equals( item.Value ) ).Key;

		Inventory.Remove( toDrop.Value );

		var go = toDrop.Value.GameObject;

		go.Transform.Position = Controller.EyePosition + Controller.EyeRotation.Forward * 10;
		go.Tags.Add( ITH.Tag.Loot );
		go.Enabled = true;


		var rb = go.Components.GetOrCreate<Rigidbody>();
		rb.Enabled = true;
		rb.ApplyForce( Controller.EyeRotation.Forward * 10 );
		return true;
	}

	// TODO:
	public bool HasUpgrade( string identifier ) => true;
}
