namespace ITH;

// TODO: We can keep this cleanly generic by just using ActionGraph and hooking into the Usable OnUse and just use Door.cs component.
public sealed class ReturnToShopDoor : Component
{
	[Property] private Usable _usable;

	protected override void OnStart()
	{
		_usable.OnUsed += Use;
		_usable.UseString = "go back to the shop";
	}

	protected override void OnUpdate()
	{
	}

	private void Use( Player user )
	{
		MansionGame.Instance.SetLevel( LevelType.Shop );
	}
}
