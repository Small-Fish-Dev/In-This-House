namespace BrickJam;

public partial class Player
{
	[Net] public bool Blocked { get; set; } = false;
	public bool CommandsLocked => IsStunned || MovementLocked || Blocked;
	public bool MovementLocked => IsTripping || IsSlipping || !IsAlive;

	[Net] public float StunDuration { get; set; } = 1.5f;
	public bool IsStunned => !StunLeft;
	[Net] public TimeUntil StunLeft { get; set; }

	public void Stun( float multiplier = 1f )
	{
		ResetStatus();

		multiplier = Math.Clamp( multiplier, 0.1f, 1f );
		var volume = MathX.Remap( multiplier, 0.1f, 1f, 0.3f, 1f );
		var pitch = MathX.Remap( multiplier, 0.1f, 1f, 1.4f, 0.5f );

		PlaySound( "sounds/crash/crash.sound" )
			.SetVolume( multiplier );

		StunLeft = StunDuration * multiplier;
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
	}

	[Net] public float SlipDuration { get; set; } = 2f;
	public bool IsSlipping => !SlipLeft;
	[Net] public TimeUntil SlipLeft { get; set; }

	public void Slip()
	{
		ResetStatus();
		//PlaySound( "sounds/pipe.sound" )
		//	.SetVolume( multiplier );

		SlipLeft = SlipDuration;

		Velocity = (Velocity.WithZ( 0 ).Normal * RunSpeed).WithZ( Velocity.z );
	}

	public void ResetStatus()
	{
		StunLeft = -1f;
		TripLeft = -1f;
		SlipLeft = -1f;
	}
}
