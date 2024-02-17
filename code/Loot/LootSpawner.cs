namespace ITH;

public sealed class LootSpawner : Component, Component.ExecuteInEditor
{
	private PrefabFile LootPrefab { get; set; }
	[Property] public bool IsContainer { get; private set; }
	[Property] public float ChanceToSpawn { get; set; } = 0.5f;
	[Property] public LevelType LevelType { get; set; } = LevelType.None;
	[Property] public LootSpawnPosition SpawnPosition { get; private set; }
	public GameObject? LootSpawned { get; set; }

	[Property]
	bool SboxMomentDetermineSpawnPositionType { get; set; } = false;

	// sbox moment
	protected override void OnEnabled()
	{
		if ( !Scene.IsEditor )
			return;

		if ( !SboxMomentDetermineSpawnPositionType )
			return;


		SpawnPosition = LootSpawnPosition.Ground;

		var trace = new PhysicsTraceBuilder();
		trace.Ray( Transform.Position + Vector3.Up * 10, Transform.Position + Vector3.Down * 12 );

		var tr = Scene.PhysicsWorld.RunTrace( trace );
		if ( !tr.Hit )
		{
			SpawnPosition = LootSpawnPosition.Wall;
		}
	}

	public void SpawnLoot()
	{
		if ( !Networking.IsHost )
			return;

		bool lootPredicate( PrefabDefinition prefab )
		{
			var loot = prefab.GetComponent<Loot>();
			var levelCanSpawn = loot.Get<LevelType>( "LevelCanAppearOn" );
			var spawnType = loot.Get<LootSpawnPosition>( "WhereCanSpawn" );
			return spawnType == SpawnPosition && Level.Current.Id == levelCanSpawn;
		}

		var interestedLoot = LootManager.Instance.Loot.Where( lootPredicate ).ToArray();
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
		Gizmo.Draw.Text( SpawnPosition.ToString(), new global::Transform( Vector3.Zero + Vector3.Down * 5, Rotation.Identity, 1 ) );

		if ( Gizmo.Control.Sphere( null, 6, out _, Color.Green ) )
		{
			Gizmo.Select( false, false );
		}
	}
}
