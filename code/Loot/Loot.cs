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
	private bool _disabling;

	protected override void OnStart()
	{
		Rarity = LootManager.RandomRarityFromLevel( Level.Current.Id );
		SellPrice = (int)(MonetaryValue * LootManager.RarityMap[Rarity]);
		_usable ??= Components.Get<Usable>();
		_usable.CanUse = true;
		_usable.OnUsed += Use;
		_usable.UseString = $"take the {FullName}";
	}

	protected override void OnUpdate()
	{
		if ( _picker == null )
		{
			Transform.Scale = Vector3.Lerp( Transform.Scale, 1f, 5f * Time.Delta );
			return;
		}

		Transform.Scale = Vector3.Lerp( Transform.Scale, 0.01f, 5f * Time.Delta );
		Log.Info( Transform.Scale.Length );
		if ( Transform.Scale.Length <= 0.5f && !_disabling )
		{
			_disabling = true;

			var item = new ItemEntry { GameObject = GameObject, Loot = this };
			if ( _picker.Inventory.Add( item ) )
			{
				Log.Info( $"You picked up: {FullName}" );
				// Eventlog.Send( $"You picked up <gray>1x {item.Name}.", To.Single( picker ) );
				GameObject.Enabled = false;

				_disabling = false;
				_picker = null;
				Transform.Scale = Vector3.One;
			}
			else
			{
				// Eventlog.Send( $"<red>No space for <gray>1x {item.Name}.", To.Single( picker ) );
				_picker = null;
				_disabling = false;
			}
		}
	}

	private void Use( Player user )
	{
		_picker = user;
		var controller = user.Controller;

		var normal = (controller.EyePosition - Transform.Position).Normal;
		var force = 512 + Transform.Position.Distance( controller.EyePosition );

		// TODO: For some reason this effect just doesn't look as good as it did in the entity system.
		// MassOverride seems to have no effect? Paintings barely move towards us but ground objects do quite fast.
		var rigidbody = Components.GetOrCreate<Rigidbody>( FindMode.EverythingInSelf );
		{
			rigidbody.Enabled = true;
			rigidbody.MassOverride = 1f;
			rigidbody.ApplyImpulse( force * (normal + Vector3.Up * 1.5f) );
		}

		// Particles.Create( "particles/smoke/smoke_steal.vpcf", Transform.Position );
		Sound.Play( "sounds/grab/grab.sound", Transform.Position );
	}
}
