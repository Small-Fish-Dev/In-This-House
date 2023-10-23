using GridAStar;
using Sandbox.Utility;

namespace BrickJam;

public partial class Specter : NPC
{
	public override string ModelPath { get; set; } = "models/specter/specter.vmdl";
	public override float WalkSpeed { get; set; } = 80f;
	public override float RunSpeed { get; set; } = 220f;
	public float TimeToTeleport => 3f; // 4f = 2 seconds to go into ground, (1 second always added to stay underground) 2 seconds to rise up
	public bool IsLowering => LastTeleport <= TimeToTeleport / 2f + 0.5f;
	public bool IsRising => LastTeleport <= TimeToTeleport + 1f && LastTeleport > TimeToTeleport / 2f + 0.5f;
	public bool IsTeleporting => IsLowering || IsRising;
	[Net] public TimeSince LastTeleport { get; set; } = 999f; // screw it
	internal CapsuleLightEntity lampLight { get; set; }
	internal Particles teleport { get; set; }

	public Specter() { }
	public Specter( Level level ) : base( level ) { }

	public override void ClientSpawn()
	{
		base.Spawn();

		//PlaySound( "sounds/wega.sound" );

		lampLight = new CapsuleLightEntity();
		lampLight.CapsuleLength = 12f;
		lampLight.LightSize = 12f;
		lampLight.Enabled = true;
		lampLight.Color = new Color( 0.5f, 0.4f, 0.2f );
		lampLight.Range = 512f;
		lampLight.Brightness = 0.4f;
		lampLight.Position = GetAttachment( "lamp" ).Value.Position;
		lampLight.SetParent( this, "lamp" );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		lampLight?.Delete();
		teleport?.Destroy();
	}

	[GameEvent.Tick.Client]
	void flickerLight()
	{
		if ( lampLight != null )
		{
			if ( !IsTeleporting )
				lampLight.Brightness = Noise.Perlin( Time.Now * 200f, 100f, 1000f );
			else
			{
				if ( IsLowering )
					lampLight.Brightness = Math.Max( MathX.Remap( LastTeleport, 0f, TimeToTeleport / 2f, 0f, -2f ) + Noise.Perlin( Time.Now * 200f, 100f, 1000f ), 0f );

				if ( IsRising )
					lampLight.Brightness = Math.Max( MathX.Remap( LastTeleport, TimeToTeleport / 2f + 1f, TimeToTeleport + 1f, -2f, 0f ) + Noise.Perlin( Time.Now * 200f, 100f, 1000f ), 0f );
			}
		}
	}
	public override void ComputeIdleAndSeek()
	{
		if ( !IsTeleporting )
		{
			teleport?.Destroy();

			if ( InVision.Count > 0 )
			{
				Target = InVision.OrderBy( x => x.Key.Position.Distance( Position ) )
				.FirstOrDefault().Key;
				if ( Target is Player player )
					if ( player.Doob != null )
						Target = player.Doob;

				LastTarget = Target;
			}
			else
				Target = null;

			if ( Target == null )
			{
				if ( !IsFollowingPath )
				{
					if ( nextIdle )
					{
						var isLongIdle = MansionGame.Random.Float() <= 0.2f;

						Cell chosenCell = null;
						var tried = 0;
						while ( chosenCell != null && isLongIdle ? chosenCell.Position.Distance( Position ) < 1000f : chosenCell?.Position.Distance( Position ) > 400f || chosenCell == null )
						{
							chosenCell = MansionGame.Random.FromList( Level.Grid?.AllCells.ToList() ) ?? null;
							tried++;

							if ( tried >= 20 )
								break;
						}

						if ( chosenCell != null )
						{
							if ( !isLongIdle )
								NavigateTo( chosenCell );
							else
								Teleport( chosenCell.Position );
						}

						nextIdle = MansionGame.Random.Float( 1f, 2f );

						LastTarget = null;
					}
				}
			}

			if ( Target != null ) // Kill player is in range
			{
				if ( Target.Position.Distance( Position ) <= KillRange )
				{
					if ( Target is Player player )
						CatchPlayer( player );
					if ( Target is Doob doobie )
						CatchDoob( doobie );
				}
			}
		}
	}

	public override void ComputeMotion()
	{
		if ( !IsTeleporting )
			base.ComputeMotion();
		else
		{
			if ( IsLowering )
				Position += Vector3.Down * CollisionHeight * Time.Delta / (TimeToTeleport / 2f);

			if ( IsRising )
				Position += Vector3.Up * CollisionHeight * Time.Delta / (TimeToTeleport / 2f);
		}
	}

	public async void Teleport( Vector3 position )
	{
		LastTeleport = 0;
		teleport = Particles.Create( "particles/dust/specter_teleport.vpcf", this, true );

		await GameTask.Delay( (int)( TimeToTeleport * 500 + 500 ));

		Position = position + Vector3.Down * CollisionHeight + Vector3.Down * CollisionHeight * Time.Delta * 0.5f;

		ResetInterpolation();
	}

	[ConCmd.Server( "Specter" )]
	public static void SpawnNPC()
	{
		if ( ConsoleSystem.Caller.Pawn is not Player player ) return;

		var npc = new Specter( MansionGame.Instance.CurrentLevel );
		npc.Position = player.Position + player.Rotation.Forward * 300f;
		npc.Rotation = player.Rotation;
	}

	[ConCmd.Server( "TpToMe" )]
	public static void TeleportSpecter()
	{
		var caller = ConsoleSystem.Caller.Pawn;

		foreach ( var specter in Entity.All.OfType<Specter>().ToList() )
			specter.Teleport( caller.Position);
	}
}
