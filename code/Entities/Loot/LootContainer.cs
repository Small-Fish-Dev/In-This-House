namespace BrickJam;

public partial class LootContainer : UsableEntity
{
	[Net] public bool Spitting { get; set; }

	public override float InteractionDuration => 2f;
	public override bool CanUse => !Spitting;
	public override string UseString => CanUse ? "open the container" : string.Empty;
	public override string LockText => "lockpick the container";
	public override bool StartLocked => true;

	private static IReadOnlyDictionary<LevelType, string> models = new Dictionary<LevelType, string>()
	{
		[LevelType.Mansion] = "models/containers/safe/safe.vmdl",
		[LevelType.Dungeon] = "models/containers/chest/chest.vmdl",
		[LevelType.Bathrooms] = "models/containers/medicine_cabinet/medicine_cabinet.vmdl"
	};

	// ANIMATINO FIX!!
	private async void funny()
	{
		await GameTask.Delay( 500 );
		SetAnimParameter( "open", true );
		await GameTask.Delay( 500 );
		SetAnimParameter( "open", false );
	}

	public override void Spawn()
	{
		if ( !models.TryGetValue( MansionGame.Instance.CurrentLevel.Type, out var model ) )
		{
			Delete();
			Log.Warning( "Failed to spawn loot container!!" );
			return;
		}

		base.Spawn();
		SetModel( model );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		Tags.Add( "solid", "container" );

		funny();
	}
	private async void spit()
	{
		Spitting = true;
		SetAnimParameter( "open", true );

		await GameTask.Delay( 100 );

		var lootCount = Game.Random.Int( 2, 5 );
		var levelLoot = LootPrefab.All
			.Where( x => x.Value.Level == MansionGame.Instance.CurrentLevel.Type )
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
			loot.Scale = 0.01f;
			loot.Rarity = (LootRarity)Math.Min( (int)loot.Rarity + 2, 8 );

			await GameTask.Delay( (int)( 1500 / lootCount ) );
		}
	}

	public override void Use( Player user )
	{
		if ( Spitting || Game.IsClient )
			return;

		spit();
	}
}
