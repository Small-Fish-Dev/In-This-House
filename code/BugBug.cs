namespace BrickJam;

/// <summary>
/// Bug squared
/// </summary>
public static class BugBug
{
	const bool ENABLED = false;

	private static readonly Stack<Action<Builder>> BuildStack = new();

	public class Builder
	{
		private int _line;
		private int _indent;

		private static readonly Vector2 Position = new(20);

		private void KeyString( string textOne, Color colorOne, string textTwo = null, Color? colorTwo = null )
		{
			var computedTextOne = "";

			if ( _indent != 0 )
			{
				computedTextOne += new string( ' ', _indent * 4 );
				computedTextOne += "- ";
			}

			computedTextOne += $"{textOne}: ";
			DebugOverlay.ScreenText( computedTextOne, Position, _line, colorOne );

			if ( textTwo != null )
			{
				var computedTextTwo = "";
				computedTextTwo += new string( ' ', computedTextOne.Length );
				computedTextTwo += textTwo;
				DebugOverlay.ScreenText( computedTextTwo, Position, _line, colorTwo ?? Color.White );
			}

			Space();
		}

		public void Group( string key, Action f )
		{
			KeyString( key, Color.Orange );
			_indent += 1;
			f.Invoke();
			_indent -= 1;
			Space();
		}

		public void Value( string key, object value ) => KeyString( key, Color.Yellow, $"{value}" );

		public void Text( string key, Color? color = null )
		{
			DebugOverlay.ScreenText( key, Position, _line, color ?? Color.White );
			Space();
		}

		public void Space() => _line++;
	}

	[GameEvent.Client.PostCamera]
	private static void Draw()
	{
		if ( !ENABLED )
			return;

		var builder = new Builder();
		while ( BuildStack.Count != 0 )
		{
			BuildStack.Pop().Invoke( builder );
		}

		Reset();
	}

	private static void Reset()
	{
		BuildStack.Clear();
	}

	public static void Here( Action<Builder> f )
	{
		BuildStack.Push( f );
	}
}
