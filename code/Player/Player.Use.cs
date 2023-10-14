using Sandbox;
using System;
using System.Linq;

namespace BrickJam;

partial class Player : AnimatedEntity
{
	public UseableEntity UseableEntity { get; set; } = null;
	public float UseRange => 120f;

	protected void SimulateUse()
	{
		UseableEntity = null;

		var thinTrace = Trace.Ray( EyePosition, EyePosition + InputRotation.Forward * UseRange )
			.WithTag( "useable" )
			.Run();

		if ( thinTrace.Entity is UseableEntity useableThin )
			UseableEntity = useableThin;

		if ( UseableEntity == null )
		{
			var thickTrace = Trace.Ray( EyePosition, EyePosition + InputRotation.Forward * UseRange )
				.Size( 12f )
				.WithTag( "useable" )
				.Run();

			if ( thickTrace.Entity is UseableEntity useableThick )
				UseableEntity = useableThick;
		}

		if ( UseableEntity == null )
		{
			var foundInSphere = FindInSphere( thinTrace.EndPosition, 20f );

			foreach ( var found in foundInSphere )
				if ( found is UseableEntity foundUseable )
					UseableEntity = foundUseable;
		}

		if ( Input.Pressed( "use" ) && Input.Down( "use" ) )
		{
			UseableEntity?.Use( this );
			Log.Warning( UseableEntity );
		}
	}
}
