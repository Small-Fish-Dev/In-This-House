namespace ITH;

[Icon( "6mp" )]
public partial class LootContainer : Component
{
	[Sync] public bool Spitting { get; set; }
	[Property] private Usable _usable;
	[Property] private Lock? _lock;
	[Property] private GameObject Test;
	private SkinnedModelRenderer _model;
	private static IReadOnlyList<PrefabFile> LootPrefabs;
	private IReadOnlyDictionary<LevelType, string> models = new Dictionary<LevelType, string>()
	{
		[LevelType.Shop] = "models/containers/safe/safe.vmdl",
		[LevelType.Mansion] = "models/containers/safe/safe.vmdl",
		[LevelType.Dungeon] = "models/containers/chest/chest.vmdl",
		[LevelType.Bathrooms] = "models/containers/medicine_cabinet/medicine_cabinet.vmdl"
	};

	protected override void OnStart()
	{
		_usable ??= Components.Get<Usable>();
		_model = Components.Get<SkinnedModelRenderer>();

		_usable.InteractionDuration = 2f;
		_usable.LockText = "lockpick the container";
		_usable.StartLocked = true;

		if ( !models.TryGetValue( MansionGame.Instance.CurrentLevel.Id, out var model ) )
		{
			Log.Info( model );
			GameObject.Destroy();
			Log.Warning( "Failed to spawn loot container!!" );
			return;
		}

		_model.Model = Model.Load( model );
		_usable.OnUsed += Use;
	}

	protected override void OnUpdate()
	{
		if ( Scene.IsEditor )
			return;

		_usable.CanUse = !Spitting;
		_usable.UseString = _usable.CanUse ? "open the container" : string.Empty;
	}

	protected override void OnDestroy()
	{
		if ( Networking.IsHost )
			_lock?.GameObject.Destroy();

		_usable.OnUsed -= Use;
		models = null;
	}

	private async void SpitLoot()
	{
		Log.Info( "spitting loot" );
		Spitting = true;
		_model.Set( "open", true );

		await GameTask.Delay( 100 );

		var lootCount = Game.Random.Int( 2, 5 );

		var allLoot = PrefabLibrary.FindByComponent<Loot>();
		// TODO: Figure out some caching instead of doing this stuff every time we spit loot. Unless it's not a big hit or something.
		var levelLoot = allLoot.Where( x => x.GetComponent<Loot>().Get<LevelType>( "LevelCanAppearOn" ) == MansionGame.Instance.CurrentLevel.Id ).ToArray();

		var def = levelLoot.FirstOrDefault();

		for ( int i = 0; i < lootCount; i++ )
		{
			var transform = _model.GetAttachment( "exit" ) ?? Transform.World;
			var normal = (transform.Forward + Vector3.Random / 8f).WithZ( 0 );
			var force = 100f;

			var prefab = SceneUtility.GetPrefabScene( Game.Random.FromArray( levelLoot, def ).Prefab );
			var lootGameObject = prefab.Clone( transform.Position, Game.Random.Rotation() );
			if ( lootGameObject.Components.TryGet<Rigidbody>( out var rb, FindMode.EverythingInSelf ) )
			{
				rb.Enabled = true;
				rb.ApplyForce( force * normal + Vector3.Up * 300f );
			}

			// loot.Scale = 0.01f;
			var loot = lootGameObject.Components.Get<Loot>();
			loot.Rarity = (LootRarity)Math.Min( (int)loot.Rarity + 2, 8 );

			await GameTask.Delay( (int)(1500 / lootCount) );
		}
	}

	public void Use( Player user )
	{
		if ( Spitting || !Networking.IsHost )
			return;

		SpitLoot();
	}
}