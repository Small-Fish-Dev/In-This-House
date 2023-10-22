using Editor;

namespace BrickJam;

[HammerEntity]
[EditorModel( "models/furniture/shop/shop_counter.vmdl" )]
public partial class Shop : UsableEntity
{
	public override bool ShouldCenterInteractionHint => false;
	public override float InteractionDuration => 0f;
	public override string UseString => "open the upgrades shop";

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

		user.Velocity = Vector3.Zero;
	}

	[ClientRpc]
	internal void OpenShop()
	{
		if ( !UI.Shop.IsOpen )
			UI.Shop.Open();
	}
}
