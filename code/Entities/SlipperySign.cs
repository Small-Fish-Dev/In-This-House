using Editor;

namespace BrickJam;

[HammerEntity]
[EditorModel( "models/furniture/bathrooms_furniture/wet_floor_sign.vmdl" )]
public partial class SlipperySign : ModelEntity
{
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/furniture/bathrooms_furniture/wet_floor_sign.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		Tags.Add( "slip", "nocollide" );
	}
}
