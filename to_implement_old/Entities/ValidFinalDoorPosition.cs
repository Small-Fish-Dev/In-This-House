using Editor;

namespace BrickJam;

[HammerEntity]
[EditorModel( "models/furniture/exit_door.vmdl" )]
public partial class ValidFinalDoorPosition : Entity
{
	public ValidFinalDoorPosition() => Transmit = TransmitType.Never;
}
