namespace ITH;

/// <summary>
/// Things belonging to our local player's instance.
/// </summary>
public static class Local
{
	public static GameObject Pawn;
	public static Player Player;
	public static Player? Spectated;
	public static PlayerController PlayerController;
	public static Client Client;

	// sbox moment
	public static void DestroyAll()
	{
		Pawn = null;
		Player = null;
		PlayerController = null;
		Client = null;
		Spectated = null;
	}
}
