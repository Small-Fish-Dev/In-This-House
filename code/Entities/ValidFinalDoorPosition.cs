using Editor;

namespace BrickJam;

[HammerEntity]
[EditorModel( "models/furniture/mansion_furniture/mansion_door.vmdl" )]
public partial class ValidFinalDoorPosition : Entity
{
	public ValidFinalDoorPosition() => Transmit = TransmitType.Never;
}
