namespace BrickJam;

public class UsableEntity : AnimatedEntity
{
	[BindComponent] public LockedComponent Lock { get; }

	public virtual float InteractionDuration => 1.0f;
	
	/// <summary>
	/// Whether the interaction circle should be at the world center of the entity
	/// </summary>
	public virtual bool ShouldCenterInteractionHint => true;
	public virtual string UseString => $"interact with {GetType().FullName}";
	public virtual bool CanUse => true;
	public virtual bool StartLocked => false;
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

		var attachment = model?.GetAttachment( "lock" );
		if ( Game.IsServer && StartLocked && attachment != null )
			Components.GetOrCreate<LockedComponent>()?.Initialize();
	}
}
