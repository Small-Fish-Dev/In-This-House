using Sandbox.Effects;

namespace BrickJam;

partial class MansionGame
{
	public ScreenEffects ScreenEffects { get; private set; }

	[Event.Hotload]
	protected void InitializeEffects()
	{
		if ( Game.IsServer )
			return;

		// s&box's own post processing effects, we might want to use chromatic aberration and vignette.
		ScreenEffects = Camera.Main.FindOrCreateHook<ScreenEffects>();
		ScreenEffects.Vignette.Intensity = 0.4f;

		// dithering
		var dithering = Camera.Main.FindOrCreateHook<DitheringEffect>();
		dithering.Order = -1;
	}
}
