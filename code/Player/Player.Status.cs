namespace BrickJam;

public partial class Player
{
	public float StunDuration => 2f;
	public bool IsStunned => !StunLeft;
	[Net] public TimeUntil StunLeft { get; set; }

	public void Stun()
	{
		Log.Error( "TODO: play a loud thud!" );

		StunLeft = StunDuration;
	}
}
