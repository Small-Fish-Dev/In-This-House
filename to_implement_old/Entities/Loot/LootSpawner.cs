using Editor;
using Sandbox.Internal;

namespace BrickJam;

[HammerEntity]
public partial class LootSpawner : Entity
{
	[Property]
	public LootPrefab LootToSpawn { get; set; }

	[Property]
	public bool IsContainer { get; set; } = false;

	[Property]
	public float ChanceToSpawn { get; set; } = 0.5f;
	[Property] public LevelType LevelType { get; set; } = LevelType.None;

	public IEntity EntitySpawned { get; set; }

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Never;
	}

	public void SpawnLoot()
	{
		var chance = MansionGame.Random.Float();

		if ( chance <= ChanceToSpawn * Game.Clients.Count() * 0.25f )
		{
			if ( IsContainer )
			{
				EntitySpawned = new LootContainer()
				{
					Position = Position,
					Rotation = Rotation,
				};

				return;
			}

			if ( LootToSpawn == null )
				return;

			EntitySpawned = Loot.CreateFromGameResource( LootToSpawn, Position, Rotation );

			if ( EntitySpawned is null )
				Log.Error( $"{this} Couldn't spawn item! item: {LootToSpawn}" );
		}
	}

	public void DeleteLoot() => EntitySpawned?.Delete();

	#region HAMMER GIZMO
	private static LootPrefab gizmoPrefab;
	private static string gizmoPath;

	public static void DrawGizmos( EditorContext context )
	{
		var container = context.Target.GetProperty( "IsContainer" ).As.Bool;
		if ( container )
		{
			Gizmo.Draw.Model( "models/containers/safe/safe.vmdl" );
			return;
		}

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
