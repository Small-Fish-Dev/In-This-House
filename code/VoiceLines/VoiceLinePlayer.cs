namespace BrickJam.VoiceLines;

public partial class VoiceLinePlayer : Entity
{
	public class PlayedVoiceLine
	{
		public Sound CurrentSound { get; set; }
		public Entity AttachedEntity { get; set; }
		public VoiceLine VoiceLine { get; set; }
		public TimeSince Playing { get; set; }
	}

	public static VoiceLinePlayer The => (VoiceLinePlayer)_instance?.Target;
	private static WeakReference _instance;

	public IReadOnlyList<PlayedVoiceLine> CurrentVoiceLines => _voiceLines;

	private List<PlayedVoiceLine> _voiceLines = new();

	public VoiceLinePlayer()
	{
		_instance = new WeakReference( this );
	}

	[GameEvent.Tick.Client]
	public void Tick()
	{
		var before = CurrentVoiceLines.Count;
		// Remove all the voice lines that are either over or the entity that one was attached to no longer exists
		_voiceLines.RemoveAll( playedVoiceLine =>
			playedVoiceLine.Playing > 2f && !playedVoiceLine.CurrentSound.IsPlaying // TODO: hack
			|| playedVoiceLine.AttachedEntity is not null && !playedVoiceLine.AttachedEntity.IsValid );
		var after = CurrentVoiceLines.Count;
		if ( after < before )
			Log.Info( $"Removed {before - after} voice lines!" );
	}

	public void Clear()
	{
		foreach ( var playedVoiceLine in CurrentVoiceLines )
		{
			playedVoiceLine.CurrentSound.Stop();
		}

		_voiceLines = new List<PlayedVoiceLine>();
	}

	/// <summary>
	/// Play a voice line
	/// </summary>
	/// <param name="caller">null if the sound has no position</param>
	/// <param name="filepath">path to the voice line resource</param>
	[ClientRpc]
	public static void Play( Entity caller, string filepath )
	{
		if ( ResourceLibrary.TryGet<VoiceLine>( filepath, out var loadedVoiceLine ) )
		{
			The.Play( caller, loadedVoiceLine );
		}
		else
		{
			Log.Error( $"Could not find a voice line \"{filepath}\"" );
		}
	}

	public void Play( Entity caller, VoiceLine voiceLine )
	{
		Game.AssertClient();

		var i = _voiceLines.FindIndex( other =>
			caller == other.AttachedEntity
			&& voiceLine.ResourcePath == other.VoiceLine.ResourcePath );

		var pvl = new PlayedVoiceLine { VoiceLine = voiceLine, AttachedEntity = caller };

		if ( i == -1 )
		{
			_voiceLines.Add( pvl );
		}
		else
		{
			pvl = _voiceLines[i];
			_voiceLines.RemoveAt( i );
			_voiceLines.Add( pvl );

			if ( pvl.CurrentSound.IsPlaying )
				pvl.CurrentSound.Stop();
		}

		Log.Info( $"{pvl.VoiceLine.LocalizedSound.ResourcePath}" );

		pvl.CurrentSound = caller.IsValid()
			? Sound.FromEntity( pvl.VoiceLine.LocalizedSound.ResourcePath, caller, null )
			: Sound.FromScreen( pvl.VoiceLine.LocalizedSound.ResourcePath );
		pvl.Playing = 0;

		Event.Run( VoiceLineEvent.VoiceLineEventName, pvl );
	}

	[ConCmd.Client]
	static void ClearVoiceLines()
	{
		The.Clear();
	}

	[ConCmd.Client]
	static void PlayVoiceLineUI( string filepath )
	{
		Play( null, filepath );
	}

	[ConCmd.Client]
	static void PlayVoiceLineSelf( string filepath )
	{
		if ( ConsoleSystem.Caller is null || ConsoleSystem.Caller.Pawn is not Entity self || !self.IsValid() )
			return;

		Play( self, filepath );
	}

	[ConCmd.Client]
	static void PlayVoiceLineLookAt( string filepath )
	{
		if ( ConsoleSystem.Caller is null || ConsoleSystem.Caller.Pawn is not Player pawn || !pawn.IsValid() )
			return;

		var trace = Trace.Ray( pawn.EyePosition, pawn.EyePosition + pawn.Rotation.Forward * 100 )
			.Ignore( pawn )
			.Run();

		if ( !trace.Hit || trace.Entity is not { } entity )
		{
			Log.Error( "Didn't hit an entity" );
			return;
		}

		Play( entity, filepath );
	}
}
