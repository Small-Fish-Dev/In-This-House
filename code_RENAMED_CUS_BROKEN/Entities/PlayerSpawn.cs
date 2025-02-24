using Editor;

namespace BrickJam;

[HammerEntity]
[EditorModel( "models/citizen/citizen.vmdl" )]
public partial class PlayerSpawn : Entity
{
	[Property] public LevelType LevelType { get; set; } = LevelType.None;

	public PlayerSpawn() => Transmit = TransmitType.Never;
}
