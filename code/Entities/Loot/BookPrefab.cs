using System.Text.Json.Serialization;

namespace BrickJam;

[GameResource( "BookPrefab", "book", "Define book data.", Icon = "star" )]
public class BookPrefab : LootPrefab
{
	public string Text { get; set; }

	[ResourceType( "vmdl" )] public new string Model { get; set; } = "models/sbox_props/watermelon/watermelon_gib08.vmdl";
}
