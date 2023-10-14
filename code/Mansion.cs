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

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		var pawn = new Player();
		client.Pawn = pawn;
		pawn.Respawn();
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );
		
		BugBug.Here( v =>
		{
			v.Text( "small fish jam game" );
			v.Value( "time", DateTime.Now );
			v.Space();
			
			v.Group( "local camera", () =>
			{
				v.Value( "pos", Camera.Position );
				v.Value( "ang", Camera.Rotation.Angles() );
			} );
		} );
	}
}
