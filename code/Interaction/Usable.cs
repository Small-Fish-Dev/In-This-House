namespace ITH;

public sealed class Usable : Component
{
	[Property] public string UseString = "interact";
	[Property] public bool StartLocked;
	[Property] public Action<PlayerController> OnUsed;
	public Lock? Lock => GameObject.Components.Get<Lock>();
	public float InteractionDuration => 1.0f;
	public bool ShouldCenterInteractionHint => true;
	public bool CanUse { get; set; }
	public bool Locked => Lock?.Locked ?? false;
	public PlayerController User { get; set; }
	public string LockText { get; set; }
	public bool CheckUpgrades( PlayerController player ) => true;
}
