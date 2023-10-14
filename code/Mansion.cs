global using Sandbox;
global using System.Linq;
global using System.Threading.Tasks;
global using System.Collections.Generic;
global using System;
global using Sandbox.UI.Construct;
global using System.IO;

namespace BrickJam;

public partial class Mansion : GameManager
{
	public static Mansion Instance { get; private set; }

	public Mansion()
	{
		Instance = this;
		
		if ( Game.IsClient )
			_ = new Hud();
	}

	/// <summary>
	/// A client has joined the server. Make them a pawn to play with
	/// </summary>
	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		// Create a pawn for this client to play with
		var pawn = new Player();
		client.Pawn = pawn;

		// Get all of the spawnpoints
		var spawnpoints = Entity.All.OfType<SpawnPoint>();

		// chose a random one
		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		// if it exists, place the pawn there
		if ( randomSpawnPoint != null )
		{
			var tx = randomSpawnPoint.Transform;
			tx.Position = tx.Position + Vector3.Up * 50.0f; // raise it up
			pawn.Transform = tx;
		}
	}
}
