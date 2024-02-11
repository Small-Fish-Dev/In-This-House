namespace BrickJam;

public partial class Spectator
{
	public float WalkSpeed => 200f;
	public float RunSpeed => 350f;
	public float SpectatorRadius => 15f;

	static string[] ignoreTags = { "player", "npc", "nocollide", "door" };

	protected void SimulateController()
	{
		IsRunning = Input.Down( "run" );
		var wishSpeed = IsRunning ? RunSpeed : WalkSpeed;
		var wishVelocity = InputDirection.WithZ( Input.Down( "jump" ) ? 1 : 0 ).Normal * InputRotation * wishSpeed;

		var lerpVelocity =
			Velocity.LerpTo( wishVelocity,
				(wishVelocity.LengthSquared > Velocity.LengthSquared ? 15f : 5f) // Accelerate faster than decelerate
				* Time.Delta );

		var helper = new MoveHelper( Position, lerpVelocity );
		helper.Trace = Trace
			.Sphere( SpectatorRadius, Position, Position )
			.WithoutTags( ignoreTags )
			.Ignore( this );

		helper.TryMove( Time.Delta );
		helper.TryUnstuck();

		Position = helper.Position;
		Velocity = helper.Velocity;
	}
}
