using Editor;

namespace BrickJam;

[HammerEntity]
[EditorModel( "models/furniture/shop/shop_counter.vmdl" )]
public partial class Shop : UsableEntity
{
	public override bool ShouldCenterInteractionHint => false;
	public override float InteractionDuration => 0.7f;
	public override string UseString => "open the upgrades shop";

	private Vector3? _lastShopOpenPos;
	private const float MoveLeeway = 10;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/furniture/shop/shop_counter.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		Tags.Add( "solid" );
	}

	public override void Use( Player user )
	{
		base.Use( user );

		OpenShop( To.Single( user.Client ) );
		Sound.FromWorld( "sounds/store/store.sound", user.Position + user.Rotation.Forward * 50f );

		user.Velocity = Vector3.Zero;
	}

	[ClientRpc]
	internal void OpenShop()
	{
		if ( !UI.Shop.IsOpen )
		{
			_lastShopOpenPos = (Game.LocalPawn as Player)?.EyePosition;
			UI.Shop.Open();
		}
	}

	[GameEvent.Tick.Client]
	private void CloseIfMovedAway()
	{
		if ( _lastShopOpenPos == null )
			return;
		if ( Game.LocalPawn is not Player player )
			return;
		if ( _lastShopOpenPos.Value.Distance( player.EyePosition ) > MoveLeeway )
		{
			Log.Info( "Moved away from shop, closing" );
			UI.Shop.Close();
			_lastShopOpenPos = null;
		}
	}
}
