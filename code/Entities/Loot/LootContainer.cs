namespace BrickJam;

public class LootContainer : UsableEntity
{
	public override float InteractionDuration => 2f;
	public override string UseString => "open the container.";

	[Property]
	public float ChanceToSpawn { get; set; } = 0.5f;

	private IReadOnlyDictionary<LevelType, string> models = new Dictionary<LevelType, string>()
	{
		[LevelType.Mansion] = "models/containers/safe/safe.vmdl",
		[LevelType.Dungeon] = "models/containers/chest/chest.vmdl",
		[LevelType.Bathrooms] = "models/containers/medicine_cabinet/medicine_cabinet.vmdl"
	};

	public override void Spawn()
	{
		var random = MansionGame.Random.Float( 0f, 1f );
		if ( random < ChanceToSpawn )
		{
			Delete();
			return;
		}

		var level = MansionGame.Instance?.CurrentLevel?.Type ?? LevelType.Mansion;
		if ( !models.TryGetValue( level, out var model ) )
		{
			Delete();
			Log.Warning( "Failed to spawn loot container!!" );
			return;
		}

		SetModel( model );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		Tags.Add( "solid" );
	}

	private bool spitting = false;

	private async void spit()
	{
		var lootCount = Game.Random.Int( 2, 5 );
		var levelLoot = LootPrefab.All
			.Where( x => x.Value.Level == (MansionGame.Instance?.CurrentLevel?.Type ?? LevelType.Mansion) )
			.Select( x => x.Value )
			.ToArray();
		var def = LootPrefab.All.FirstOrDefault().Value;

		for ( int i = 0; i < lootCount; i++ )
		{
			var normal = Vector3.Random.WithZ( 1 );
			var force = 200f;

			var prefab = Game.Random.FromArray( levelLoot, def );
			var loot = Loot.CreateFromGameResource( prefab, GetAttachment( "exit" )?.Position ?? Position, Game.Random.Rotation() );
			loot.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
			loot.ApplyAbsoluteImpulse( force * normal );

			await GameTask.Delay( 500 );
		}
	}

	public override void Use( Player user )
	{
		if ( spitting || Game.IsClient )
			return;

		spit();
	}
}
