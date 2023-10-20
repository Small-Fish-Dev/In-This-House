using Editor;
using Sandbox.Internal;

namespace BrickJam;

[HammerEntity]
public partial class LootSpawner : Entity
{
	[Property]
	public LootPrefab LootToSpawn { get; set; }
	[Property]
	public float ChanceToSpawn { get; set; } = 0.5f;

	public Loot LootSpawned { get; set; }

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Never;
	}

	public void SpawnLoot()
	{
		var chance = MansionGame.Random.Float();

		if ( chance <= ChanceToSpawn && LootToSpawn != null )
		{
			LootSpawned = Loot.CreateFromGameResource( LootToSpawn, Position, Rotation );

			if ( LootSpawned is null )
				Log.Error( $"{this} Couldn't spawn item! item: {LootToSpawn}" );
		}
	}

	public void DeleteLoot() => LootSpawned?.Delete();

	#region HAMMER GIZMO
	private static LootPrefab gizmoPrefab;
	private static string gizmoPath;

	public static void DrawGizmos( EditorContext context )
	{
		var path = context.Target.GetProperty( "LootToSpawn" ).As.String;
		if ( path == null )
			return;

		if ( gizmoPath != path )
		{
			try
			{
				gizmoPrefab = GlobalGameNamespace.ResourceLibrary.Get<LootPrefab>( path );
				gizmoPath = path;
			}
			catch {}
		}

		if ( gizmoPrefab == null )
			return;

		var model = Model.Load( gizmoPrefab.Model );
		if ( model == null || model.IsError )
			return;

		Gizmo.Draw.Model( model );
	}
	#endregion
}
