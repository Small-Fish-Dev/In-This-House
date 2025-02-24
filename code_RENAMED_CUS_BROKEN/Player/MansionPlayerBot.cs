namespace BrickJam;

public class MansionPlayerBot : Bot
{
	[ConCmd.Admin( "mansion_bot_add", Help = "Spawn a mansion bot." )]
	internal static void SpawnCustomBot()
	{
		Game.AssertServer();

		// Create an instance of your custom bot.
		_ = new MansionPlayerBot();
	}

	public override void BuildInput()
	{
		// Here we can choose / modify the bot's input each tick.
		// We'll make them constantly attack by holding down the PrimaryAttack button.
		//Input.SetButton( InputButton.PrimaryAttack, true );
		// And here, we'll make the bot walk forward and turn in a wide circle.
		Input.AnalogMove = Vector3.Forward;
		Input.AnalogLook = new Angles( 0, 30 * Time.Delta, 0 );
		
		// Finally, we'll call BuildInput on the bot's client's pawn. 
		// Note that Entity.BuildInput is NOT automatically called for the pawns of
		// simulated clients that are driven by bots, so that's why we call it here.
		(Client.Pawn as Entity)?.BuildInput();
	}
	
	public override void Tick()
	{
		// TODO: do something useful
	}
}
