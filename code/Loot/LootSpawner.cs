namespace ITH;

public sealed class LootSpawner : Component
{
	private PrefabFile LootPrefab { get; set; }
	[Property] public bool IsContainer { get; private set; }
	[Property] public float ChanceToSpawn { get; set; } = 0.5f;
	[Property] public LevelType LevelType { get; set; } = LevelType.None;
	private LootSpawnPosition _spawnType;
	public GameObject? LootSpawned { get; set; }

	protected override void OnStart()
	{
		if ( !Networking.IsHost )
			return;

		_spawnType = LootSpawnPosition.Ground;

		var tr = Scene.Trace.Ray( Transform.Position, Transform.Position + Vector3.Down * 5 ).Run();
		if ( !tr.Hit )
		{
			_spawnType = LootSpawnPosition.Wall;
		}
		Log.Info( tr.Hit );
	}

	public void SpawnLoot()
	{
		if ( !Networking.IsHost )
			return;

		bool lootPredicate( PrefabDefinition prefab )
		{
			var loot = prefab.GetComponent<Loot>();
			var spawnType = loot.Get<LootSpawnPosition>( "WhereCanSpawn" );
			return spawnType == _spawnType;
		}

		var interestedLoot = LootManager.Instance.MansionLoot.Where( lootPredicate ).ToArray();
		LootPrefab = Game.Random.FromArray( interestedLoot ).Prefab;

		var chance = Random.Shared.Float();
		if ( chance <= ChanceToSpawn * MansionGame.Instance.Clients.Count() * 0.25f )
		{
			if ( IsContainer )
			{
				// EntitySpawned = new LootContainer()
				// {
				// 	Position = Position,
				// 	Rotation = Rotation,
				// };

				return;
			}

			if ( LootPrefab == null )
				return;

			LootSpawned = SceneUtility.GetPrefabScene( LootPrefab ).Clone( Transform.Position, Transform.Rotation );
			LootSpawned.NetworkSpawn();

			if ( LootSpawned is null )
				Log.Error( $"{this} Couldn't spawn item! item: {LootPrefab}" );
		}
	}

	public void DeleteLoot() => LootSpawned?.Destroy();

	protected override void DrawGizmos()
	{
		var distance = Vector3.DistanceBetween( Transform.Position, Gizmo.CameraTransform.Position );

		if ( distance >= 512 )
		{
			Gizmo.Draw.IgnoreDepth = true;
			Gizmo.Draw.LineCircle( Vector3.Zero, 8 );
			return;
		}

		// if ( LootPrefab is not null )
		// {
		// 	var model = LootPrefab.AsDefinition().GetComponent<ModelRenderer>().Get<string>( "Model" );

		// 	Gizmo.Draw.Model( model, global::Transform.Zero );
		// }

		Gizmo.Draw.Text( "LootSpawner", new global::Transform( Vector3.Zero, Rotation.Identity, 1 ) );

		if ( Gizmo.Control.Sphere( null, 6, out _, Color.Green ) )
		{
			Gizmo.Select( false, false );
		}
	}
}
