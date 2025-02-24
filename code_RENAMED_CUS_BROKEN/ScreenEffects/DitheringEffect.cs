namespace BrickJam;

public class DitheringEffect : RenderHook
{
	public override void OnStage( SceneCamera target, Stage stage )
	{
		if ( stage != Stage.AfterUI )
			return;

		var attributes = new RenderAttributes();
		var mat = Material.FromShader( "shaders/dithering_postprocess.shader" );
		Graphics.GrabFrameTexture( renderAttributes: attributes );
		Graphics.Blit( mat, attributes );
	}
}
