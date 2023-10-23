using BrickJam.UI;
using Editor;

namespace BrickJam;

[HammerEntity]
[Title( "Sell Area" ), Category( "Trigger" ), Icon( "place" )]
public partial class SellArea : BaseTrigger
{
	public SellArea()
	{
		Transmit = TransmitType.Always;
		EnableTouch = true;
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( other is not Loot loot )
			return;

		if ( loot.LastPlayer == null )
			return;

		loot.LastPlayer.SetMoney( loot.LastPlayer.Money + loot.MonetaryValue );

		Eventlog.Send( $"You sold the <gray>{loot.FullName}<white> for <green>${loot.MonetaryValue}.",
			To.Single( loot.LastPlayer ) );

		Sound.FromWorld( "sounds/store/store.sound", loot.Position + loot.Rotation.Forward * 50f );
		
		OnSellRpc( To.Everyone, loot.Position, loot.MonetaryValue );
		
		loot.Delete();
	}

	[ClientRpc]
	public static void OnSellRpc( Vector3 position, int value )
	{
		for ( var i = 0; i < 10; i++ )
		{
			var waft = new MoneyWaft( value );
			waft.Position = position;
		}
	}
}
