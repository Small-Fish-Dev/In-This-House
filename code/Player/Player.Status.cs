namespace BrickJam;

public partial class Player
{
	public bool CommandsLocked => IsStunned || IsTripping || IsSlipping;
	public bool MovementLocked => IsTripping || IsSlipping;

	[Net] public float StunDuration { get; set; } = 1.5f;
	public bool IsStunned => !StunLeft;
	[Net] public TimeUntil StunLeft { get; set; }

	public void Stun( float multiplier = 1f )
	{
		ResetStatus();

		multiplier = Math.Clamp( multiplier, 0.1f, 1f );
		var volume = MathX.Remap( multiplier, 0.1f, 1f, 0.3f, 1f );
		var pitch = MathX.Remap( multiplier, 0.1f, 1f, 1.4f, 0.5f );

		PlaySound( "sounds/pipe.sound" )
			.SetVolume( multiplier );

		StunLeft = StunDuration * multiplier;
		CancelInteraction();
	}

	[Net] public float TripDuration { get; set; } = 1.5f;
	public bool IsTripping => !TripLeft;
	[Net] public TimeUntil TripLeft { get; set; }

	public void Trip()
	{
		ResetStatus();
		//PlaySound( "sounds/pipe.sound" )
		//	.SetVolume( multiplier );

		TripLeft = TripDuration;
		CancelInteraction();
	}

	[Net] public float SlipDuration { get; set; } = 1f;
	public bool IsSlipping => !SlipLeft;
	[Net] public TimeUntil SlipLeft { get; set; }

	public void Slip()
	{
		ResetStatus();
		//PlaySound( "sounds/pipe.sound" )
		//	.SetVolume( multiplier );

		SlipLeft = SlipDuration;
	}

	public void ResetStatus()
	{
		StunLeft = -1f;
		TripLeft = -1f;
		SlipLeft = -1f;
	}
}
