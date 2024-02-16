namespace ITH;

/// <summary>
/// A band-aid re-implementation of the old Entity system era Event system.
/// </summary>
public sealed class LegacyEventSystem : Component
{
	public static LegacyEventSystem Instance;
	public Action Update;
	public Action FixedUpdate;
	public Action PreRender;
	public Action InventoryChanged;

	protected override void OnAwake()
	{
		Instance = this;
	}

	protected override void OnUpdate()
	{
		Update?.Invoke();
	}

	protected override void OnFixedUpdate()
	{
		FixedUpdate?.Invoke();
	}

	protected override void OnPreRender()
	{
		PreRender?.Invoke();
	}

	protected override void OnDestroy()
	{
		Instance = null;
	}
}
