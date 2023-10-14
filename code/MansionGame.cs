global using Sandbox;
global using System.Linq;
global using System.Threading.Tasks;
global using System.Collections.Generic;
global using System;
global using Sandbox.UI.Construct;
global using System.IO;

namespace BrickJam;

public partial class MansionGame : GameManager
{
	public static MansionGame Instance { get; private set; }

	public MansionGame()
	{
		Instance = this;

		if ( Game.IsClient )
		{
			InitializeEffects();
			_ = new Hud();
		}
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		var pawn = new Player();
		client.Pawn = pawn;
		pawn.Respawn( Instance.CurrentLevel.Type );
		// TODO: Have pawn dead for now
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
