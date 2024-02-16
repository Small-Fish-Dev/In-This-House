namespace ITH;

partial class Player
{
	public InteractionRequest CurrentInteractionRequest { get; private set; }
	public Usable CurrentUsable { get; private set; }
	public Vector3 UsableTouchPosition { get; private set; }
	public bool HasActiveInteractionRequest =>
	CurrentInteractionRequest is not null && CurrentInteractionRequest.IsActive;

	public bool HasValidInteractionRequest =>
		CurrentInteractionRequest is not null && CurrentInteractionRequest.IsValid;
	public float UseRange => 100f;

	private bool UpdateUsableEntity( float rayRadius = 0 )
	{
		CurrentUsable = null;
		UsableTouchPosition = Vector3.Zero;

		var trace = Scene.Trace.Ray( Controller.EyePosition, Controller.EyePosition + Controller.InputAngles.Forward * (UseRange - rayRadius) )
		.WithTag( ITH.Tag.Usable );
		if ( rayRadius != 0 )
			trace = trace.Size( rayRadius );

		var traceResult = trace.Run();
		if ( !traceResult.Hit )
			return false;

		if ( traceResult.GameObject.Components.TryGet<Usable>( out var usable ) )
		{
			CurrentUsable = usable;
			UsableTouchPosition = traceResult.HitPosition;
			return true;
		}

		return false;
	}

	public void UpdateUse()
	{
		// Do a pass with a thin ray, then with a 24 unit thick ray if failed
		if ( !UpdateUsableEntity() )
			UpdateUsableEntity( 12 );

		// Cancel and return if we're unable to interact with anything (stunned, tied up or what not)
		// Also cancel if the player isn't holding use
		if ( Controller.CommandsLocked || !Input.Down( GameInputActions.Use ) )
		{
			return;
		}

		// If the player has not used anything yet
		if ( CurrentUsable is not null
			 && Input.Pressed( "use" )
			&& !HasActiveInteractionRequest
			&& CurrentUsable.CanUse )
		{
			if ( CurrentUsable.Locked )
			{
				if ( HasUpgrade( "Lock Breaker" ) )
				{
					Sound.Play( "sounds/lockpicking/lockfall.sound", CurrentUsable.Lock.Transform.Position );
					CurrentUsable.Lock.Unlock();
				}
				else
				{
					CurrentUsable.Lock.Lockpick( this );
					Model.Set( "lockpicking", true );
				}
			}
			// Grab if no one uses it
			else
			{
				if ( CurrentUsable.CheckUpgrades( this ) )
					EnqueueInteraction();
			}
		}

		if ( HasValidInteractionRequest && CurrentInteractionRequest.Complete )
		{
			Model.Set( "grab", true );
			FinishInteraction();
		}

		// Remove the invalid or complete interaction request
		if ( !HasActiveInteractionRequest
				|| CurrentInteractionRequest.HitPoint.Distance( Controller.EyePosition ) > UseRange + 10 )
		{
			CancelInteraction();
		}
	}

	private void EnqueueInteraction()
	{
		CurrentInteractionRequest = new InteractionRequest( CurrentUsable, this, UsableTouchPosition );
	}

	private void FinishInteraction()
	{
		CurrentInteractionRequest.Finish();
		CurrentInteractionRequest = null;
	}

	public void CancelInteraction()
	{
		if ( !HasValidInteractionRequest )
			return;

		CurrentInteractionRequest.Release();
		CurrentInteractionRequest = null;
	}
}
