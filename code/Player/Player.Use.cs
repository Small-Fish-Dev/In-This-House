using Sandbox;

namespace BrickJam;

partial class Player : AnimatedEntity
{
	public partial class InteractionRequest : BaseNetworkable
	{
		[Net] public UsableEntity Entity { get; set; }
		[Net] public TimeUntil Complete { get; set; }
		[Net] public Vector3 HitPoint { get; set; }

		public InteractionRequest() { }

		public InteractionRequest( UsableEntity usableEntity, Player user, Vector3? hitPoint = null )
		{
			if ( !usableEntity.IsValid() )
				throw new ArgumentNullException( nameof(usableEntity) );

			Entity = usableEntity;
			Complete = usableEntity.InteractionDuration / ( user.HasUpgrade( "Faster Use" ) ? 3f : 1f );
			if ( hitPoint == null )
			{
				Log.Warning( "hitPoint == null?" );
				HitPoint = usableEntity.WorldSpaceBounds.Center;
			}
			else
			{
				HitPoint = hitPoint.Value;
			}

			usableEntity.User?.CancelInteraction();
			usableEntity.User = user;
		}

		public bool IsValid => Entity.IsValid();

		public bool IsActive => IsValid && !Complete;

		public void Finish()
		{
			if ( !Entity.IsValid() )
				return;

			Entity.Use( Entity.User );
			Release();
		}

		public void Release()
		{
			Entity.User = null;
		}
	}

	/// <summary>
	/// Entity we're currently using
	/// </summary>
	[Net]
	public InteractionRequest CurrentInteractionRequest { get; set; }

	/// <summary>
	/// A usable entity that we're looking at
	/// </summary>
	public UsableEntity UsableEntity { get; private set; }

	/// <summary>
	/// The position we see of the usable entity
	/// </summary>
	public Vector3 UsableEntityTouchPos { get; private set; }

	public bool HasActiveInteractionRequest =>
		CurrentInteractionRequest is not null && CurrentInteractionRequest.IsActive;

	public bool HasValidInteractionRequest =>
		CurrentInteractionRequest is not null && CurrentInteractionRequest.IsValid;

	public float UseRange => 100f;

	private bool UpdateUsableEntity( float rayRadius = 0 )
	{
		var trace = Trace.Ray( EyePosition, EyePosition + InputRotation.Forward * (UseRange - rayRadius) )
			.WithTag( "useable" ); // TODO: lol useable -> usable
		if ( rayRadius != 0 )
			trace = trace.Size( rayRadius );

		var traceResult = trace.Run();

		if ( traceResult.Entity is UsableEntity usable )
		{
			UsableEntity = usable;
			UsableEntityTouchPos = traceResult.HitPosition;
			return true;
		}

		UsableEntity = null;
		UsableEntityTouchPos = Vector3.Zero;
		return false;
	}

	protected void SimulateUse()
	{
		// Do a pass with a thin ray, then with a 24 unit thick ray if failed
		if ( !UpdateUsableEntity() )
			UpdateUsableEntity( 12 );

		// Third pass, wider search radius
		// rndtrash: I don't think we need it, it's very conchfusing
		/*if ( UsableEntity == null )
		{
			var foundInSphere = FindInSphere( thinTrace.EndPosition, 20f );

			foreach ( var found in foundInSphere )
				if ( found is UsableEntity foundUsable && foundUsable.CanUse )
					UsableEntity = foundUsable;
		}*/

		// Cancel and return if we're unable to interact with anything (stunned, tied up or what not)
		// Also cancel if the player isn't holding use
		if ( CommandsLocked || !Input.Down( "use" ) )
		{
			if ( Game.IsServer )
				CancelInteraction();

			return;
		}

		// If the player has not used anything yet
		if ( UsableEntity is not null
			 && Input.Pressed( "use" )
			&& !HasActiveInteractionRequest
			&& UsableEntity.CanUse )
		{
			if ( UsableEntity.Locked )
			{
				if ( Game.IsServer )
				{
					if ( HasUpgrade( "Lock Breaker" ) )
					{
						Sound.FromWorld( "sounds/lockpicking/lockfall.sound", UsableEntity.Lock.Lock.Position );
						UsableEntity.Lock.Unlock();
					}
					else
					{
						UsableEntity.Lock.Lockpick( this );
						SetAnimParameter( "lockpicking", true );
					}
				}
			}
			// Grab if no one uses it
			else
			{
				if ( Game.IsServer )
					if ( UsableEntity.CheckUpgrades( this ) )
						EnqueueInteraction();
			}
		}

		if ( Game.IsServer )
		{
			if ( HasValidInteractionRequest && CurrentInteractionRequest.Complete )
				FinishInteraction();

			// Remove the invalid or complete interaction request
			if ( !HasActiveInteractionRequest
					|| CurrentInteractionRequest.HitPoint.Distance( EyePosition ) > UseRange + 10 )
			{
				CancelInteraction();
			}
		}
	}

	private void EnqueueInteraction()
	{
		Game.AssertServer();

		CurrentInteractionRequest = new InteractionRequest( UsableEntity, this, UsableEntityTouchPos );
	}

	private void FinishInteraction()
	{
		Game.AssertServer();

		CurrentInteractionRequest.Finish();
		CurrentInteractionRequest = null;
	}

	private void CancelInteraction()
	{
		Game.AssertServer();

		if ( !HasValidInteractionRequest )
			return;

		CurrentInteractionRequest.Release();
		CurrentInteractionRequest = null;
	}
}
