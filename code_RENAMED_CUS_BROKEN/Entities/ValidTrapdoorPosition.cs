using Editor;

namespace BrickJam;

[HammerEntity]
[EditorModel( "models/furniture/trap_door.vmdl" )]
public partial class ValidTrapdoorPosition : Entity
{
	[Property] public LevelType LevelType { get; set; } = LevelType.None;

	public ValidTrapdoorPosition() => Transmit = TransmitType.Never;
}
