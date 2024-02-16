namespace ITH;

public enum DoorState : sbyte
{
	Opening = 1,
	Closing = -1,
	Open = 2,
	Closed = 3,
}

/// <summary>
/// 
/// </summary>

// TODO: We can keep this cleanly generic by just using ActionGraph and hooking into the Usable OnUse.
public sealed class Door : Component
{
	[Property] private Usable _usable;
	[Property] private ModelRenderer _model;
	[Sync] public DoorState State { get; set; } = DoorState.Closed;
	[Sync] public bool Locked { get; set; }

	private static IReadOnlyDictionary<LevelType, (string, string)> models = new Dictionary<LevelType, (string, string)>()
	{
		[LevelType.Mansion] = ("models/furniture/mansion_furniture/mansion_door.vmdl", "models/furniture/mansion_furniture/mansion_door.vmdl"),
		[LevelType.Dungeon] = ("models/furniture/dungeon_props/dungeon_wood_door.vmdl", "models/furniture/dungeon_props/dungeon_jail_door.vmdl"),
		[LevelType.Bathrooms] = ("models/furniture/dungeon_props/bathroom_stall_door.vmdl", "models/furniture/dungeon_props/bathroom_stall_door.vmdl")
	};

	private Transform initialTransform;
	private Vector3? _hinge;
	private int side;

	private Vector3 hinge
	{
		get
		{
			if ( _hinge == null )
				_hinge = Transform.Position.WithZ( _model.Bounds.Center.z ) + _model.Bounds.Maxs.WithZ( 0 );

			return _hinge.Value;
		}
	}

	protected override void OnAwake()
	{
		initialTransform = Transform.World;
		_usable ??= Components.Get<Usable>();
		_usable.CanUse = true;
		_usable.OnUsed += Use;
	}

	private void Use( Player user )
	{
		if ( State == DoorState.Open || State == DoorState.Opening )
			Close();
		else
			Open( user );
	}

	public void Open( Player user )
	{
		State = DoorState.Opening;

		var localPosition = Transform.World.PointToLocal( user.Transform.Position );
		side = localPosition.y > 0 ? -1 : 1;

		Sound.Play( "sounds/doors/dooropen.sound" );
	}

	public void Close()
	{
		State = DoorState.Closing;
	}

	protected override void OnFixedUpdate()
	{
		// TODO: I do think it was better before when this was handled via a getter instead of every-tick logic. 
		// Maybe make Usable abstract and inherit from it like before? It's probably not a problem, probably.
		_usable.UseString = (State == DoorState.Open || State == DoorState.Opening) ? "close the door" : "open the door";

		// No need to update if we are static.
		if ( State == DoorState.Open || State == DoorState.Closed || !Networking.IsHost )
			return;

		const float TIME = 0.25f;
		const float ANGLE = 90f;
		const float PUSH_FORCE = 50f;

		// Calculate desired angles.
		var state = (int)State;
		var yaw = initialTransform.Rotation.Yaw()
			+ Math.Max( state, 0 ) * ANGLE * side;

		var targetRotation = initialTransform.Rotation
			.Angles()
			.WithYaw( yaw )
			.ToRotation();

		// Snap the door.
		if ( Transform.Rotation.Distance( targetRotation ).AlmostEqual( 0, 1f ) )
		{
			State = State == DoorState.Opening
				? DoorState.Open
				: DoorState.Closed;

			Transform.Rotation = targetRotation;

			if ( State == DoorState.Closed )
				Sound.Play( "sounds/doors/doorclose.sound" );

			OccupyCells();
			return;
		}

		// TODO: Prevent colliding with players.

		// var center = CollisionWorldSpaceCenter;
		// var trace = Trace.Body( PhysicsBody, center + Rotation.Left * 4f * state )
		// 	.WithAnyTags( "player" )
		// 	.Run();

		// // Push the player if we hit one.
		// if ( trace.Hit )
		// {
		// 	if ( trace.Entity != null && trace.Entity.IsValid )
		// 		trace.Entity.Velocity += -trace.Normal * PUSH_FORCE;

		// 	return;
		// }

		// Linear rotation for the door.
		var rot = Rotation.Lerp( Transform.Rotation, targetRotation, 1f / TIME * Time.Delta );
		Transform.Rotation = rot;
	}

	public void OccupyCells()
	{
		// TODO:
	}
}
