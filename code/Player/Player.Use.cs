using Sandbox;

namespace BrickJam;

partial class Player : AnimatedEntity
{
	public partial class InteractionRequest : BaseNetworkable
	{
		[Net] public UsableEntity Entity { get; set; }
		[Net] public TimeUntil Complete { get; set; }

		public InteractionRequest() { }

		public InteractionRequest( UsableEntity usableEntity, Player user )
		{
			if ( !usableEntity.IsValid() )
				throw new ArgumentNullException( nameof(usableEntity) );

			Entity = usableEntity;
			Complete = usableEntity.InteractionDuration;

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

	public bool HasActiveInteractionRequest =>
		CurrentInteractionRequest is not null && CurrentInteractionRequest.IsActive;
	
	public bool HasValidInteractionRequest =>
		CurrentInteractionRequest is not null && CurrentInteractionRequest.IsValid;

	public float UseRange => 100f;

	private UsableEntity FindUsableEntity( float rayRadius = 0 )
	{
		var trace = Trace.Ray( EyePosition, EyePosition + InputRotation.Forward * (UseRange - rayRadius) )
			.WithTag( "useable" ); // TODO: lol useable -> usable
		if ( rayRadius != 0 )
			trace = trace.Size( rayRadius );

		var traceResult = trace.Run();

		if ( traceResult.Entity is UsableEntity usable )
			return usable;

		return null;
	}

	protected void SimulateUse()
	{
		// Do a pass with a thin ray, then with a 24 unit thick ray if failed
		UsableEntity = FindUsableEntity() ?? FindUsableEntity( 12f );

		// Third pass, wider search radius
		// rndtrash: I don't think we need it, it's very conchfusing
		/*if ( UsableEntity == null )
		{
			var foundInSphere = FindInSphere( thinTrace.EndPosition, 20f );

			foreach ( var found in foundInSphere )
				if ( found is UsableEntity foundUsable && foundUsable.CanUse )
					UsableEntity = foundUsable;
		}*/
		if ( UsableEntity != null ) 
		DebugOverlay.Text( UsableEntity.Name, UsableEntity.Position );

		if ( Game.IsServer )
		{
			// Cancel and return if we're unable to interact with anything (stunned, tied up or what not)
			// Also cancel if the player isn't holding use
			if ( CommandsLocked || !Input.Down( "use" ) )
			{
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
					UsableEntity.Lock.Lockpick( this );
				// Grab if no one uses it
				else if ( !UsableEntity.User.IsValid() )
					EnqueueInteraction( UsableEntity );
			}

			if ( HasValidInteractionRequest && CurrentInteractionRequest.Complete )
				FinishInteraction();
			
			// Remove the invalid or complete interaction request
			if ( !HasActiveInteractionRequest
			     || CurrentInteractionRequest.Entity.Position.Distance( Position ) > UseRange * 2f )
			{
				CancelInteraction();
			}
		}
	}

	private void EnqueueInteraction( UsableEntity entity )
	{
		Game.AssertServer();

		CurrentInteractionRequest = new InteractionRequest( entity, this );
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
