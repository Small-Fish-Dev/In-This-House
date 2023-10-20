using Editor;

namespace BrickJam;

[HammerEntity]
[EditorModel( "models/containers/safe/safe.vmdl" )]
public partial class LootContainer : UsableEntity
{
	[Net] public bool Spitting { get; set; }

	public override float InteractionDuration => 2f;
	public override bool CanUse => !Spitting;
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
		Tags.Add( "solid", "container" );
	}

	private async void spit()
	{
		Spitting = true;
		SetAnimParameter( "open", true );

		var lootCount = Game.Random.Int( 2, 5 );
		var levelLoot = LootPrefab.All
			.Where( x => x.Value.Level == (MansionGame.Instance?.CurrentLevel?.Type ?? LevelType.Mansion) )
			.Select( x => x.Value )
			.ToArray();
		var def = LootPrefab.All.FirstOrDefault().Value;

		for ( int i = 0; i < lootCount; i++ )
		{
			var transform = GetAttachment( "exit" ) ?? Transform;
			var normal = (transform.Forward + Vector3.Random / 8f).WithZ( 0 );
			var force = 100f;

			var prefab = Game.Random.FromArray( levelLoot, def );
			var loot = Loot.CreateFromGameResource( prefab, transform.Position, Game.Random.Rotation() );
			loot.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
			loot.ApplyAbsoluteImpulse( force * normal + Vector3.Up * 300f );

			await GameTask.Delay( 1250 );
		}
	}

	public override void Use( Player user )
	{
		if ( Spitting || Game.IsClient )
			return;

		spit();
	}
}
