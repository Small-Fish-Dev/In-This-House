using Sandbox;
using System;
using System.Linq;

namespace BrickJam;

partial class Player : AnimatedEntity
{
	/// <summary>
	/// Entity we're currently using
	/// </summary>
	[Net]
	public UsableEntity CurrentlyUsedEntity { get; set; }

	/// <summary>
	/// How long have we been interacting with UsableEntity
	/// </summary>
	[Net]
	public TimeUntil InteractionComplete { get; set; }

	/// <summary>
	/// A usable entity that we're looking at
	/// </summary>
	public UsableEntity UsableEntity { get; set; }

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
				if ( found is UsableEntity foundUsable )
					UsableEntity = foundUsable;
		}

		// Cancel and return if we're unable to interact with anything (stunned, tied up or what not)
		if ( !CanUse )
		{
			CancelInteraction();
			return;
		}

		if ( CurrentlyUsedEntity is not null )
		{
			// If we are not looking at the same entity as frame earlier or we have released the use key
			// then we should cancel the interaction.
			if ( CurrentlyUsedEntity != UsableEntity || !Input.Down( "use" ) )
				CancelInteraction();

			if ( InteractionComplete )
				FinishInteraction();
		}
		else
		{
			if ( UsableEntity is not null && Input.Down( "use" ) )
			{
				CurrentlyUsedEntity = UsableEntity;
				InteractionComplete = UsableEntity.InteractionDuration;
			}
		}
	}

	protected void FinishInteraction()
	{
		if ( CurrentlyUsedEntity is null )
			return;

		CurrentlyUsedEntity.Use( this );
		CurrentlyUsedEntity = null;
	}

	protected void CancelInteraction()
	{
		CurrentlyUsedEntity = null;
		InteractionComplete = 0;
	}
}
