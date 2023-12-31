﻿namespace BrickJam.Upgrading;

public class Upgrade
{
	private static readonly List<Upgrade> BuiltUpgrades = new();
	public static IEnumerable<Upgrade> All => BuiltUpgrades.AsReadOnly();

	private Upgrade() { }

	/// <summary> You probably don't need this! </summary>
	public static Upgrade CreateEmptyUpgrade() => new();

	public static Upgrade Find( string identifier ) => All.FirstOrDefault( v => v.Identifier == identifier );

	public static bool Exists( string identifier ) => Find( identifier ) != null;

	public static void ClearAll() => BuiltUpgrades.Clear();

	private Upgrade( string identifier, string title, string description )
	{
		Identifier = identifier;
		Title = title;
		Description = description;
	}

	public string Identifier { get; }
	public string Title;
	public string Description;
	public string Texture;
	public int Price;

	/// <summary> Position of upgrade on skill tree, with 0 being the center </summary>
	public Vector2 Position = Vector2.Zero;

	public List<string> Dependencies;

	#region Effects

	// Example
	[Effect] public float FrictionMultiplier { get; set; } = 1f;
	[Effect] public float UseMultiplier { get; set; } = 1f;
	[Effect] public float BreakLocks { get; set; } = 1f;
	[Effect] public float Doob { get; set; } = 1f;
	[Effect] public float KeyToDungeon { get; set; } = 1f;
	[Effect] public float KeyToBathrooms { get; set; } = 1f;
	[Effect] public float KeyToExit { get; set; } = 1f;

	#endregion

	/// <summary>
	/// Forward all effects of this upgrade to another one
	/// </summary>
	/// <param name="upgrade">Upgrade</param>
	public void ForwardEffects( Upgrade upgrade )
	{
		// todo(gio): optimize
		var description = TypeLibrary.GetType( GetType() );
		foreach ( var property in description.Properties )
		{
			if ( !property.HasAttribute<EffectAttribute>() ) continue;
			if ( property.PropertyType == typeof(float) )
			{
				var value = (float)property.GetValue( this );
				var valueTwo = (float)property.GetValue( upgrade );
				property.SetValue( upgrade, value + valueTwo );
			}
			else
			{
				throw new Exception( $"Unsupported type {property.PropertyType}" );
			}
		}
	}

	public override string ToString() => $"{Identifier} ({Title}): {Dependencies?.Count} deps";

	public struct Builder
	{
		private List<Builder> _next;
		private readonly string _identifier;
		private readonly string _title;
		private readonly string _description;
		private int _price = 5;

		private Action<Upgrade> _postBuild;

		private readonly string _dependency;
		private string _last;
		private string _texture;
		private Vector2 _position;

		public Builder( string identifier, string description )
		{
			_identifier = identifier;
			_title = identifier;
			_description = description;
			_last = identifier;
		}

		private Builder( string identifier, string description, string dependency ) : this( identifier, description )
		{
			_dependency = dependency;
		}

		public Builder ConfigureWith( Action<Upgrade> action )
		{
			_postBuild = action;
			return this;
		}

		public Builder PlaceAt( Vector2 position )
		{
			_position = position;
			return this;
		}

		public Builder WithTexture( string texture, bool full = false )
		{
			_texture = full ? texture : $"textures/upgrades/{texture}";
			return this;
		}

		public Builder WithPrice( int v )
		{
			_price = v;
			return this;
		}

		public Builder Next( string identifier, Func<Builder, Builder> creator = null )
		{
			_next ??= new List<Builder>();
			var builder = new Builder( identifier, _description, _last ) { _postBuild = _postBuild };
			var next = creator?.Invoke( builder ) ?? builder;
			_last = next._identifier;
			_next.Add( next );
			return this;
		}

		public Upgrade Build()
		{
			var instance =
				new Upgrade( _identifier, _title, _description )
				{
					Position = _position, Texture = _texture, Price = _price
				};
			_postBuild?.Invoke( instance );

			instance.Dependencies ??= new List<string>();

			if ( _dependency != null )
				instance.Dependencies.Add( _dependency );

			BuiltUpgrades.Add( instance );

			if ( _next == null )
				return instance;

			foreach ( var builder in _next )
				builder.Build();

			return instance;
		}
	}
}
