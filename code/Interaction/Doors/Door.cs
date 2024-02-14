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
		_usable ??= Components.Get<Usable>();
		_usable.OnUsed += Use;

	}

	private void Use( Player user )
	{
	}
}
