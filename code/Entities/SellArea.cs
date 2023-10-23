using Editor;

namespace BrickJam;

[HammerEntity]
[Title( "Sell Area" ), Category( "Trigger" ), Icon( "place" )]
public class SellArea : BaseTrigger
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

		loot.Delete();
	}
}
