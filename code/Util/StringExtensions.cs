namespace ITH;

public static class StringExtensions
{
	public static bool EqualsOrContains( this string self, string other )
	{
		var lower = self.ToLowerInvariant();
		return lower == other || lower.Contains( other );
	}
}
