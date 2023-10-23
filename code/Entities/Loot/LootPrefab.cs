using System.Text.Json.Serialization;

namespace BrickJam;

[GameResource( "LootPrefab", "loot", "Define loot data.", Icon = "star" )]
public class LootPrefab : GameResource
{
	public static IReadOnlyDictionary<string, LootPrefab> All => all;
	private static Dictionary<string, LootPrefab> all = new();

	public string Name { get; set; }
	public string Description { get; set; }
	public LevelType Level { get; set; }

	[ResourceType( "vmdl" )]
	public string Model { get; set; }
	public int MonetaryValue { get; set; }
	public bool DisplayFront { get; set; }

	[Category( "Icon" )]
	public Vector3 IconOffset { get; set; }

	[Category( "Icon" )]
	public Angles IconAngles { get; set; }

	[JsonIgnore, HideInEditor]
	public Texture Icon { get; private set; }
	private static List<LootPrefab> queue = new();

	protected override void PostLoad()
	{
		if ( all.ContainsKey( ResourceName ) )
			return;

		all.Add( ResourceName, this );

		if ( Icon == null && !queue.Contains( this ) )
		{
			Icon = Texture.CreateRenderTarget()
				.WithSize( 256 )
				.WithScreenFormat()
				.Create();

			queue.Add( this );
		}
	}

	protected override void PostReload()
	{
		if ( !queue.Contains( this ) )
		{
			Icon?.Dispose();
			Icon = Texture.CreateRenderTarget()
				.WithSize( 256 )
				.WithScreenFormat()
				.Create();

			queue.Add( this );
		}
	}

	/// <summary>
	/// Gets an LootPrefab by the ResourceName.
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public static LootPrefab Get( string name )
	{
		if ( All.TryGetValue( name.ToLower(), out var prefab ) )
			return prefab;

		return null;
	}

	private void renderIcon( bool display = false )
	{
		// Create our scene.
		if ( string.IsNullOrEmpty( Model ) )
			return;
			
		var world = new SceneWorld();
		var camera = new SceneCamera()
		{
			World = world,
			Size = Icon.Size,
			FieldOfView = 50f,
			Position = Vector3.Backward * 10f,
			Rotation = Rotation.Identity,
			ZNear = 5,
			ZFar = 1000,
			BackgroundColor = Color.Transparent
		};

		_ = new SceneObject( world, Model, new Transform( IconOffset, IconAngles.ToRotation() ) );

		_ = new SceneLight( world, Vector3.Up * 15f + Vector3.Backward * 5f, 150f, Color.White * 1 );
		_ = new SceneLight( world, Vector3.Up * 25f + Vector3.Forward * 10f, 150f, Color.White * 1 );
		_ = new SceneLight( world, Vector3.Backward * 5f + Vector3.Down * 35f, 150f, Color.White * 1f );

		// Render to texture.
		Graphics.RenderToTexture( camera, Icon );

		if ( display )
		{
			var rect = new Rect( 250, Icon.Size );
			DebugOverlay.Texture( Icon, rect, 3 );
			DebugOverlay.Texture( Texture.Invalid, new Rect( 250, Icon.Size ), 3 );
		}

		// Dispose the scene.
		world.Delete();
	}

	[Event( "render" )]
	private static void renderIcons()
	{
		for ( int i = 0; i < queue.Count; i++ )
		{
			var prefab = queue[i];
			prefab?.renderIcon();
			queue.Remove( prefab );
		}
	}
}
