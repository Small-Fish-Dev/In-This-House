using Editor;

namespace BrickJam;

[HammerEntity]
[EditorModel( "models/placeholders/placeholder_trapdoor.vmdl" )]
public partial class ValidTrapdoorPosition : Entity
{
	[Property, Net] public LevelType LevelType { get; set; } = LevelType.None;
}
