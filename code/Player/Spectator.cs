using Sandbox.Component;

namespace BrickJam;

public partial class Spectator : AnimatedEntity
{
	/// <summary>
	/// The player that is being observed. `null` if no one is observed.
	/// </summary>
	[Net, Local]
	public Player Following { get; set; }

	public Player Body { get; set; }

	[Net, Local] protected int LastFollowIndex { get; set; } = -1;

	[ClientInput] public Vector3 InputDirection { get; protected set; }
	[ClientInput] public Angles InputAngles { get; protected set; }
	public bool IsRunning { get; protected set; }
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

		if ( Game.IsServer )
		{
			if ( Input.Pressed( "StopFollowing" ) )
				StopFollowing();
			// Follow the next available person if the pawn we've observed no longer exists
			else if ( Input.Pressed( "FollowNext" ) || Following is not null && !Following.IsValid )
				FollowNext();
			else if ( Input.Pressed( "FollowPrevious" ) ) // TODO: FollowPrevious doesn't work for some reason
				FollowPrevious();
		}

		if ( Following is null )
			SimulateController();
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		if ( Following is not null )
		{
			Camera.Position = Following.EyePosition;
			Camera.Rotation = Following.Rotation;
			Camera.FirstPersonViewer = Following;
		}
		else
		{
			Camera.Position = Position;
			Camera.Rotation = InputRotation;
			Camera.FirstPersonViewer = this;
		}

		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );
		EnableDrawing = false; // Let's use this for now
	}

	private void FollowNext()
	{
		var clients = Game.Clients.Where( client => client.Pawn is Player ).ToList();
		if ( clients.Count == 0 )
		{
			StopFollowing();
			return;
		}

		if ( Following is null )
			LastFollowIndex = 0; // Follow the first player in the list
		else
		{
			var index = clients.FindIndex( client => client.Pawn == Following );
			if ( index == -1 )
				LastFollowIndex = Math.Max( LastFollowIndex, 0 ) % clients.Count;
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
			StopFollowing();
			return;
		}

		if ( Following is null )
			LastFollowIndex = clients.Count - 1; // Follow the last player in the list
		else
		{
			var index = clients.FindIndex( client => client.Pawn == Following );
			if ( index == -1 )
				LastFollowIndex = Math.Max( LastFollowIndex, 0 ) % clients.Count;
			else
				LastFollowIndex = (index == 0 ? clients.Count - 1 : index - 1) % clients.Count;
		}

		Following = clients[LastFollowIndex].Pawn as Player;
	}

	private void StopFollowing()
	{
		if ( Following.IsValid() )
		{
			Position = Following.EyePosition;
			Rotation = Following.Rotation;
		}

		Following = null;
		LastFollowIndex = -1;
	}
}
