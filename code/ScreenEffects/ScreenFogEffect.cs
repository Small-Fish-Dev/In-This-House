namespace BrickJam;

public class ScreenFogEffect : RenderHook
{
	public Color Color { get; set; } = new Color( 0.5f ).WithAlpha( 1f );
	public float MaximumDistance { get; set; } = 7000f;
	public float MinimumDistance { get; set; } = 80f;
	public float MaxOpacity { get; set; } = 0.6f;

	public override void OnStage( SceneCamera target, Stage stage )
	{
		if ( stage != Stage.AfterTransparent )
			return;

		var mat = Material.FromShader( "shaders/screenfogpostprocess.shader" );

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
