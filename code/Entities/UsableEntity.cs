namespace BrickJam;

public class UsableEntity : ModelEntity
{
	public virtual float InteractionDuration => 1.0f;
	public virtual string UseString => $"interact with {GetType().FullName}";
	public virtual bool CanUse => true;
	
	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "useable" );
	}

	public virtual void Use( Player user )
	{
		//Log.Info( $"I was used by {user.Name}" );
	}
}
