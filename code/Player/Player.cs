namespace ITH;

public sealed class Player : Component
{
	[Property] public SkinnedModelRenderer Model { get; set; }

	protected override void OnStart()
	{
		if ( !IsProxy )
		{
			if ( !Scene.IsEditor )
				Model.RenderType = ModelRenderer.ShadowRenderType.ShadowsOnly;
		}
	}
}
