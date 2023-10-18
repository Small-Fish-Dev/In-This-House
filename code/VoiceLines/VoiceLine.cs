using System.Text.Json.Serialization;

namespace BrickJam.VoiceLines;

[GameResource( "Voice Line", "voli",
	"Describes a voice line that has a subtitle attached. Supports multiple languages." )]
public class VoiceLine : GameResource
{
	public string DefaultLanguage { get; set; }
	public Dictionary<string, SoundEvent> Sounds { get; set; }
	public Dictionary<string, string> Subtitles { get; set; }

	[JsonIgnore]
	public bool IsValidVoiceLine => DefaultLanguage != default
	                                && (Sounds?.ContainsKey( DefaultLanguage ) ?? false)
	                                && (Subtitles?.ContainsKey( DefaultLanguage ) ?? false);
	
	[HideInEditor, JsonIgnore]
	public SoundEvent LocalizedSound => Sounds.TryGetValue( Language.SelectedCode, out var userLanguageSound )
		? userLanguageSound
		: Sounds[DefaultLanguage];
	
	[HideInEditor, JsonIgnore]
	public string LocalizedSubtitle => Subtitles.TryGetValue( Language.SelectedCode, out var userLanguageSubtitle )
		? userLanguageSubtitle
		: Subtitles[DefaultLanguage];

	protected override void PostLoad()
	{
		base.PostLoad();

		if ( !IsValidVoiceLine )
			Log.Error(
				$"voice line \"{ResourceName}\" lacks a sound or a subtitle for the language {DefaultLanguage}" );
	}
}
