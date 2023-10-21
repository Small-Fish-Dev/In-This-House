using Sandbox;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace BrickJam;

partial class Player : AnimatedEntity
{
	public partial class InteractionRequest : BaseNetworkable
	{
		[Net] public UsableEntity Entity { get; set; }
		[Net] public TimeUntil Complete { get; set; }
	}

	/// <summary>
	/// Entity we're currently using
	/// </summary>
	[Net]
	public IList<InteractionRequest> InteractionRequests { get; set; }

	/// <summary>
	/// A usable entity that we're looking at
	/// </summary>
	public UsableEntity UsableEntity { get; private set; }

	public float UseRange => 120f;

	protected void SimulateUse()
	{
		UsableEntity = null;

		// First pass, looking directly at the object we want to use
		var thinTrace = Trace.Ray( EyePosition, EyePosition + InputRotation.Forward * UseRange )
			.WithTag( "useable" )
			.Run();

		if ( thinTrace.Entity is UsableEntity usableThin )
			UsableEntity = usableThin;

		// Second pass, looking somewhere around the object we want to use
		if ( UsableEntity == null )
		{
			var thickTrace = Trace.Ray( EyePosition, EyePosition + InputRotation.Forward * UseRange )
				.Size( 12f )
				.WithTag( "useable" )
				.Run();

			if ( thickTrace.Entity is UsableEntity usableThick )
				UsableEntity = usableThick;
		}

		// Third pass, wider search radius
		if ( UsableEntity == null )
		{
			var foundInSphere = FindInSphere( thinTrace.EndPosition, 20f );

			foreach ( var found in foundInSphere )
				if ( found is UsableEntity foundUsable && foundUsable.CanUse )
					UsableEntity = foundUsable;
		}

		if ( Game.IsServer )
		{
			// Cancel and return if we're unable to interact with anything (stunned, tied up or what not)
			// Also cancel if the player isn't holding use
			if ( CommandsLocked || !Input.Down( "use" ) )
			{
				CancelInteractions();
				return;
			}

			if ( UsableEntity is not null && Input.Pressed( "use" ) )
			{
				// Grab if no one uses it
				if ( UsableEntity.Locked )
					UsableEntity.Lock.Lockpick( this );
				else if ( !UsableEntity.User.IsValid() )
					EnqueueInteraction( UsableEntity );
				// Interact with the item twice to cancel
				else if ( UsableEntity.User == this )
				{
					var ir = InteractionRequests.FirstOrDefault(
						interactionRequest => interactionRequest.Entity == UsableEntity
					);
					if ( ir is not null )
						InteractionRequests.Remove( ir );

					CancelInteraction( UsableEntity );
				}
			}

			// Go through each interaction request, remove those that are either finished or not valid 
			foreach ( var interactionRequest in InteractionRequests.Where( ShouldRemoveEntityFromQueue ).ToList() )
				InteractionRequests.Remove( interactionRequest );
		}
	}

	private bool ShouldRemoveEntityFromQueue( InteractionRequest interactionRequest )
	{
		// Remove the invalid interaction requests
		if ( !interactionRequest.Entity.IsValid()
		     || interactionRequest.Entity.Position.Distance( Position ) > UseRange )
		{
			CancelInteraction( interactionRequest.Entity );
			return true;
		}

		if ( !interactionRequest.Complete )
			return false;

		FinishInteraction( interactionRequest.Entity );
		return true;
	}

	private void EnqueueInteraction( UsableEntity entity )
	{
		Game.AssertServer();

		entity.User = this;
		InteractionRequests.Add( new InteractionRequest { Complete = entity.InteractionDuration, Entity = entity } );
	}

	private void FinishInteraction( UsableEntity entity )
	{
		Game.AssertServer();

		entity.Use( this );
		entity.User = null;
	}

	private void CancelInteraction( UsableEntity entity )
	{
		Game.AssertServer();

		if ( entity.IsValid() )
			entity.User = null;
	}

	private void CancelInteractions()
	{
		foreach ( var interactionRequest in InteractionRequests )
			CancelInteraction( interactionRequest.Entity );

		InteractionRequests.Clear();
	}
}
