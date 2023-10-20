namespace BrickJam;

public class UsableEntity : ModelEntity
{
	[BindComponent]
	public LockedComponent Lock { get; }

	public virtual float InteractionDuration => 1.0f;
	public virtual string UseString => $"interact with {GetType().FullName}";
	public virtual bool CanUse => true;
	public virtual bool Locked => Lock?.Locked ?? false;
	public virtual string LockText => $"lockpick {GetType().FullName}";
	public Player User { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "useable" );
	}

	public virtual void Use( Player user )
	{
		//Log.Info( $"I was used by {user.Name}" );
	}

	public override void OnNewModel( Model model )
	{
		base.OnNewModel( model );

		if ( Game.IsServer && model.GetAttachment( "lock" ) != null )
			Components.GetOrCreate<LockedComponent>();
	}
}
