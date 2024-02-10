namespace ITH;

public sealed class Dithering : Component, Component.ExecuteInEditor
{
	private IDisposable renderHook;

	protected override void OnAwake()
	{
		var camera = Components.Get<CameraComponent>();
		renderHook = camera.AddHookAfterUI( "Dithering", 100, new Action<SceneCamera>( RenderEffect ) );
	}

	private void RenderEffect( SceneCamera camera )
	{
		var attributes = new RenderAttributes();
		var mat = Material.FromShader( "shaders/dithering_postprocess.shader" );
		Graphics.GrabFrameTexture( renderAttributes: attributes );
		Graphics.Blit( mat, attributes );
	}
}
