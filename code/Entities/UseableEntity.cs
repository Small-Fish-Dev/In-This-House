namespace BrickJam;

public class UseableEntity : ModelEntity
{
	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "useable" );
	}

	public virtual void Use()
	{

	}
}
