namespace ITH;

public sealed class ScreenFogEffect : Component, Component.ExecuteInEditor
{
	public Color Color { get; set; } = new Color( 0.5f ).WithAlpha( 1f );
	public float MaximumDistance { get; set; } = 7000f;
	public float MinimumDistance { get; set; } = 80f;
	public float MaxOpacity { get; set; } = 0.6f;
	private IDisposable renderHook;

	protected override void OnAwake()
	{
		var camera = Components.Get<CameraComponent>();
		renderHook = camera.AddHookAfterTransparent( "ScreenFog", 100, new Action<SceneCamera>( RenderEffect ) );
	}

	private void RenderEffect( SceneCamera camera )
	{
		if ( !camera.EnablePostProcessing )
			return;

		var mat = Material.FromShader( "shaders/screenfog_postprocess.shader" );
		var attributes = new RenderAttributes();
		attributes.Set( "Color", Color );
		attributes.Set( "MinDistance", MinimumDistance );
		attributes.Set( "MaxDistance", MaximumDistance );
		attributes.Set( "Opacity", MaxOpacity );

		Graphics.GrabFrameTexture( renderAttributes: attributes );
		Graphics.GrabDepthTexture( renderAttributes: attributes );

		Graphics.Blit( mat, attributes );
	}
}
