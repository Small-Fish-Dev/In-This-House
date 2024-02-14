namespace ITH;

public class InteractionRequest
{
	public Usable Usable { get; set; }
	public TimeUntil Complete { get; set; }
	public Vector3 HitPoint { get; set; }

	public InteractionRequest() { }

	public InteractionRequest( Usable usableEntity, Player user, Vector3? hitPoint = null )
	{
		if ( !usableEntity.IsValid() )
			throw new ArgumentNullException( nameof( usableEntity ) );

		Usable = usableEntity;
		Complete = usableEntity.InteractionDuration / (user.HasUpgrade( "Faster Use" ) ? 3f : 1f);
		if ( hitPoint == null )
		{
			Log.Warning( "hitPoint == null?" );
			HitPoint = usableEntity.Transform.Position;
		}
		else
		{
			HitPoint = hitPoint.Value;
		}

		usableEntity.User?.Controller.CancelInteraction();
		usableEntity.User = user;
	}

	public bool IsValid => Usable.IsValid();

	public bool IsActive => IsValid && !Complete;

	public void Finish()
	{
		if ( !Usable.IsValid() )
			return;

		// Entity.Use( Entity.User );
		Usable.OnUsed?.Invoke( Usable.User );
		Log.Info( "Use" );
		Log.Info( Complete );
		Release();

	}

	public void Release()
	{
		Usable.User = null;
	}
}
