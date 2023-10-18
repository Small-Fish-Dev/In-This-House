namespace BrickJam.VoiceLines;

[MethodArguments(typeof(VoiceLinePlayer.PlayedVoiceLine))]
public class VoiceLineEvent : EventAttribute
{
	public const string VoiceLineEventName = "voiceline";

	public VoiceLineEvent() : base( VoiceLineEventName ) { }
}
