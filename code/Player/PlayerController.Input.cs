namespace ITH;

partial class PlayerController
{
	[Sync] public Vector3 InputDirection { get; private set; }
	[Sync] public Angles InputAngles { get; private set; }
	[Sync] public Vector3 Velocity { get; private set; }
	[Sync] public bool IsCrouching { get; private set; }
	[Sync] public bool IsRunning { get; private set; }
	[Sync] public bool LockpickerActive { get; private set; }

	private void BuildInput()
	{
		if ( IsProxy )
			return;

		InputDirection = Input.AnalogMove;
		InputAngles += Input.AnalogLook * Time.Delta * Preferences.Sensitivity * 16;
		InputAngles = InputAngles.WithPitch( MathX.Clamp( InputAngles.pitch, -80.0f, 80f ) );

		if ( !CommandsLocked )
		{
			if ( !LockpickerActive )
			{
				InputDirection = Input.AnalogMove;

				InputAngles += Input.AnalogLook;
				InputAngles = InputAngles.WithPitch( Math.Clamp( InputAngles.pitch, -80f, 80f ) );
			}
			else
			{
				InputDirection = 0;
			}
		}

		if ( !MovementLocked && !LockpickerActive )
		{
			IsRunning = Input.Down( "run" );
			IsCrouching = Input.Down( "crouch" );
		}
		else
		{
			IsRunning = false;
			IsCrouching = false;
		}
	}
}
