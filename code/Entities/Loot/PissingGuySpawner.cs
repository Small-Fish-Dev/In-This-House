using Editor;
using Sandbox.Internal;
using System.Threading;

namespace BrickJam;

[HammerEntity]
[EditorModel( "models/pissing_guy/pissing_guy.vmdl" )]
public partial class PissingGuySpawner : Entity
{
	[Property]
	public float ChanceToSpawn { get; set; } = 0.3f;

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Never;
	}
	public PissingGuy SpawnGuy()
	{
		var chance = MansionGame.Random.Float();

		if ( chance <= ChanceToSpawn )
		{
			var guy = new PissingGuy( MansionGame.Instance.CurrentLevel );
			guy.Position = Position;
			guy.Rotation = Rotation;
			guy.StartingPosition = Position;
			guy.StartingRotation = Rotation;

			return guy;
		}

		return null;
	}
}
