namespace ITH;

public enum LootRarity
{
	Broken,
	Decrepit,
	Worn,
	Dusty,
	Common,
	Nice,
	Great,
	Excellent,
	Flawless
}

public enum LootSpawnPosition
{
	Wall,
	Ground
}

[Icon( "currency_exchange" )]
public partial class Loot : Component
{
	[Property] public string Name { get; private set; }
	[Property] public string Description { get; private set; }
	[Property] public LevelType LevelCanAppearOn { get; private set; }
	[Property] public LootSpawnPosition WhereCanSpawn { get; private set; }
	[Property] public int MonetaryValue { get; private set; }
	[Property] public bool DisplayFront { get; private set; }
	[Property] private Usable _usable { get; set; }
	public LootRarity Rarity { get; set; }
	public int SellPrice { get; private set; }
	public string FullName => $"{Rarity} {Name}";
	private Player _picker;
	private bool _deleting;

	protected override void OnStart()
	{
		Rarity = LootManager.RandomRarityFromLevel( Level.Current.Id );
		SellPrice = (int)(MonetaryValue * LootManager.RarityMap[Rarity]);
		_usable ??= Components.Get<Usable>();
		_usable.CanUse = true;
		_usable.OnUsed += Use;
		_usable.UseString = $"take the {FullName}";
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( _picker == null )
		{
			Transform.Scale = Vector3.Lerp( Transform.Scale, 1f, 5f * Time.Delta );
			return;
		}

		Transform.Scale = Vector3.Lerp( Transform.Scale, 0.01f, 5f * Time.Delta );
		if ( Transform.Scale.AlmostEqual( 0.01f ) && !_deleting )
		{
			_deleting = true;

			var item = new ItemEntry { Loot = this };
			if ( _picker.Inventory.Add( item ) )
			{
				// Eventlog.Send( $"You picked up <gray>1x {item.Name}.", To.Single( picker ) );
				Destroy();
			}
			else
			{
				// Eventlog.Send( $"<red>No space for <gray>1x {item.Name}.", To.Single( picker ) );
				_picker = null;
				_deleting = false;
			}
		}
	}

	private void Use( Player user )
	{
		Log.Info( user );
		_picker = user;
		var controller = user.Controller;

		var normal = (controller.EyePosition - Transform.Position).Normal;
		var force = 100f + Transform.Position.Distance( controller.EyePosition );
		if ( Components.TryGet<Rigidbody>( out var rigidbody, FindMode.EverythingInSelf ) )
		{
			rigidbody.Enabled = true;
			rigidbody.ApplyImpulse( force * (normal + Vector3.Up * 0.5f) );
		}

		user.Inventory.Add( new ItemEntry() { Loot = this }, 1 );
		Log.Info( $"You picked up: {FullName}" );
		// Particles.Create( "particles/smoke/smoke_steal.vpcf", Transform.Position );
		Sound.Play( "sounds/grab/grab.sound", Transform.Position );
	}
}
