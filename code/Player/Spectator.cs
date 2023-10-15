namespace BrickJam;

public partial class Spectator : AnimatedEntity
{
	/// <summary>
	/// The player that is being observed. `null` if no one is observed.
	/// </summary>
	[Net, Local]
	public Player Following { get; set; }

	[Net, Local] protected int LastFollowIndex { get; set; } = -1;

	[ClientInput] public Vector3 InputDirection { get; protected set; }
	[ClientInput] public Angles InputAngles { get; protected set; }
	public Rotation InputRotation => InputAngles.ToRotation();

	public override void BuildInput()
	{
		InputDirection = Input.AnalogMove;

		InputAngles += Input.AnalogLook;
		InputAngles = InputAngles.WithPitch( Math.Clamp( InputAngles.pitch, -89.9f, 89.9f ) );
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		// Follow the next available person if the pawn we've observed no longer exists
		if ( Following is not null && !Following.IsValid
		     || Input.Pressed( "FollowNext" ) )
		{
			FollowNext();
		}
		else if ( Input.Pressed( "FollowPrevious" ) )
		{
			FollowPrevious();
		}
		
		// TODO: move if not following anyone 
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		if ( Following is not null )
		{
			Camera.Position = Following.EyePosition;
			Camera.Rotation = Following.InputRotation; // TODO: does it even work?
		}
		else
		{
			Camera.Position = Position;
			Camera.Rotation = InputRotation;
		}
		
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );
		Camera.FirstPersonViewer = this; // Doesn't work?
		EnableDrawing = false; // Let's use this for now
	}

	private void FollowNext()
	{
		var clients = Game.Clients.Where( client => client.Pawn is Player ).ToList();
		if ( clients.Count == 0 )
		{
			Following = null;
			LastFollowIndex = -1;
			return;
		}

		if ( Following is not null )
		{
			var index = clients.FindIndex( client => client.Pawn == Following );
			if ( index == -1 )
				LastFollowIndex %= clients.Count;
			else
				LastFollowIndex = (index + 1) % clients.Count;
		}

		Following = clients[LastFollowIndex].Pawn as Player;
	}

	private void FollowPrevious()
	{
		var clients = Game.Clients.Where( client => client.Pawn is Player ).ToList();
		if ( clients.Count == 0 )
		{
			Following = null;
			LastFollowIndex = -1;
			return;
		}

		if ( Following is not null )
		{
			var index = clients.FindIndex( client => client.Pawn == Following );
			if ( index == -1 )
				LastFollowIndex %= clients.Count;
		}

		Following = clients[LastFollowIndex].Pawn as Player;
	}
}
