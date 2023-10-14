using Sandbox.Effects;

namespace BrickJam;

partial class MansionGame
{
	public ScreenEffects ScreenEffects { get; private set; }

	protected void InitializeEffects()
	{
		// s&box's own post processing effects, we might want to use chromatic aberration and vignette.
		ScreenEffects = Camera.Main.FindOrCreateHook<ScreenEffects>();

		// dithering
		var dithering = Camera.Main.FindOrCreateHook<DitheringEffect>();
	}
}
