namespace BrickJam;

public class UseableEntity : ModelEntity
{
	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "useable" );
	}

	public virtual void Use( Player user )
	{
		Log.Info( $"I was used by {user.Name}" );
	}
}
