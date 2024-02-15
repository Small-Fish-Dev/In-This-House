namespace ITH;

public sealed class Usable : Component
{
	[Property] public string UseString = "interact";
	[Property] public bool StartLocked;
	[Property] public Action<Player> OnUsed;
	public Lock? Lock => GameObject.Components.Get<Lock>();
	public float InteractionDuration { get; set; } = 1.0f;
	public bool ShouldCenterInteractionHint => true;
	public bool CanUse { get; set; }
	public bool Locked => Lock?.Locked ?? false;
	public Player User { get; set; }
	public string LockText { get; set; }
	public bool CheckUpgrades( Player player ) => true;

	public Vector3 GetBoundsCenter()
	{
		if ( Components.TryGet<ModelRenderer>( out var modelRenderer ) )
		{
			return modelRenderer.Bounds.Center;
		}

		if ( Components.TryGet<SkinnedModelRenderer>( out var skinnedModelRenderer ) )
		{
			return skinnedModelRenderer.Bounds.Center;
		}

		if ( Components.TryGet<Collider>( out var collider ) )
		{
			return collider.KeyframeBody.GetBounds().Center;
		}

		return Transform.Position;
	}
}
