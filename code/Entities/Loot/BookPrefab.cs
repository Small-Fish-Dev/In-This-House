using System.Text.Json.Serialization;

namespace BrickJam;

[GameResource( "BookPrefab", "book", "Define book data.", Icon = "star" )]
public class BookPrefab : LootPrefab
{
	public string Text { get; set; }
}
