namespace ITH;

public sealed class LootSpawner : Component
{
	[Property] private GameObject LootPrefab { get; set; }
	[Property] public bool IsContainer { get; private set; }
	[Property] public float ChanceToSpawn { get; set; } = 0.5f;
	[Property] public LevelType LevelType { get; set; } = LevelType.None;

	public GameObject? LootSpawned { get; set; }

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

			LootSpawned = LootPrefab.Clone( Transform.Position, Transform.Rotation );
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

		Gizmo.Draw.Text( "LootSpawner", new global::Transform( Vector3.Zero, Rotation.Identity, 1 ) );
		if ( Gizmo.Control.Sphere( null, 6, out _, Color.Green ) )
		{
			Gizmo.Select( false, false );
		}
	}
}
