namespace ITH;

partial class PlayerController
{
	[Sync] private Vector3 InputDirection { get; set; }
	[Sync] private Angles InputAngles { get; set; }
	[Sync] public Vector3 Velocity { get; private set; }
	[Sync] public bool IsCrouching { get; private set; }
	[Sync] public bool IsRunning { get; private set; }

	private void BuildInput()
	{
		if ( IsProxy )
			return;
		/* 
				if ( !CommandsLocked )
				{
					if ( !Lockpicker.Active )
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

				if ( !MovementLocked && !Lockpicker.Active )
				{
					IsRunning = Input.Down( "run" );
					IsCrouching = Input.Down( "crouch" );
				}
				else
				{
					IsRunning = false;
					IsCrouching = false;
				} */
	}
}
