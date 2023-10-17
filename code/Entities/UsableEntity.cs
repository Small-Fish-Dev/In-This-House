namespace BrickJam;

public class UsableEntity : ModelEntity
{
	public virtual float InteractionDuration => 1.0f;
	
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
