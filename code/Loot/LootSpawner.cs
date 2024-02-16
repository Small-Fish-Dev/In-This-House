namespace ITH;

public sealed class LootSpawner : Component
{
	private PrefabFile LootPrefab { get; set; }
	[Property] public bool IsContainer { get; private set; }
	[Property] public float ChanceToSpawn { get; set; } = 0.5f;
	[Property] public LevelType LevelType { get; set; } = LevelType.None;
	private LootSpawnPosition _spawnType;
	public GameObject? LootSpawned { get; set; }
	private static List<PrefabDefinition> _allLoot;

	protected override void OnStart()
	{
		_spawnType = LootSpawnPosition.Ground;

		var tr = Scene.Trace.Ray( Transform.Position + Vector3.Up * 2, Transform.Position + Vector3.Down * 5 ).IgnoreGameObjectHierarchy( GameObject ).Run();
		if ( !tr.Hit )
		{
			_spawnType = LootSpawnPosition.Wall;
		}

		_allLoot ??= PrefabLibrary.FindByComponent<Loot>().ToList();
		foreach ( var x in _allLoot )
		{
			Log.Info( x.Name );
		}

		bool lootPredicate( PrefabDefinition prefab )
		{
			var loot = prefab.GetComponent<Loot>();
			var spawnType = loot.Get<LootSpawnPosition>( "WhereCanSpawn" );
			var levelCanSpawn = loot.Get<LevelType>( "LevelCanAppearOn" );
			// Log.Info( $"{loot}, {spawnType}, {levelCanSpawn}" );
			return spawnType == _spawnType && levelCanSpawn == Level.Current.Id;
		}

		var spawnable = _allLoot.Where( lootPredicate ).ToList();
		LootPrefab = Game.Random.FromList( spawnable ).Prefab;
	}


	public void SpawnLoot()
	{
		if ( !Networking.IsHost )
			return;

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
			Log.Info( LootSpawned );

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
