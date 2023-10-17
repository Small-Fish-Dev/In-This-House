namespace BrickJam;

public partial class Player
{
	public float StunDuration => 1.5f;
	public bool IsStunned => !StunLeft;
	public bool CanUse => !IsStunned;
	[Net] public TimeUntil StunLeft { get; set; }

	public void Stun( float multiplier = 1f )
	{
		multiplier = Math.Clamp( multiplier, 0.1f, 2f);
		PlaySound( "sounds/pipe.sound" )
			.SetVolume( multiplier );

		StunLeft = StunDuration * multiplier;
		CancelInteraction();
	}
}
