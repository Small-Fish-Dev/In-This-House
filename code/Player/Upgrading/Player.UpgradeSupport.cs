﻿using BrickJam.Upgrading;

namespace BrickJam;

public partial class Player
{
	[Net, Change( "NetUpgradesEvent" )] public IList<string> Upgrades { get; set; }

	private static Upgrade _combinedUpgrades;
	public static Upgrade CombinedUpgrades => _combinedUpgrades;

	private void NetUpgradesEvent()
	{
		CombineUpgrades();
	}

	[ClientRpc]
	private void OnUpgradeRpc( string identifier )
	{
		Event.Run( "UpgradeBought", identifier );
	}

	public void CombineUpgrades()
	{
		_combinedUpgrades = Upgrade.CreateEmptyUpgrade();
		foreach ( var identifier in Upgrades )
		{
			var upgrade = Upgrade.Find( identifier );
			if ( upgrade == null )
				return;
			upgrade.ForwardEffects( _combinedUpgrades );
		}
	}

	public void AddUpgrade( string identifier )
	{
		Game.AssertServer();
		if ( !Upgrade.Exists( identifier ) )
		{
			Log.Warning( $"Unknown upgrade {identifier}" );
			return;
		}

		Upgrades.Add( identifier );
		CombineUpgrades();
		Event.Run( "UpgradeBought", identifier );
		OnUpgradeRpc( To.Single( this ), identifier );
	}

	public void RemoveUpgrade( string identifier )
	{
		Game.AssertServer();
		if ( !Upgrade.Exists( identifier ) )
		{
			Log.Warning( $"Unknown upgrade {identifier}" );
			return;
		}

		Upgrades.Remove( identifier );
		CombineUpgrades();
	}

	public bool HasUpgrade( string identifier ) => Upgrades.Contains( identifier );

	[ConCmd.Server( "ith_buyupgrade" )]
	public static void BuyUpgrade( string identifier )
	{
		Game.AssertServer();

		if ( ConsoleSystem.Caller.Pawn is not Player caller )
			return;

		var upgrade = Upgrade.Find( identifier );
		if ( upgrade == null )
		{
			Log.Warning( $"{ConsoleSystem.Caller.Name} tried to buy unknown upgrade {identifier}" );
			return;
		}

		foreach ( var upgradeDependency in upgrade.Dependencies.Where( upgradeDependency =>
			         !caller.HasUpgrade( upgradeDependency ) ) )
		{
			Log.Warning(
				$"{ConsoleSystem.Caller.Name} doesn't have dependency {upgradeDependency} for upgrade {identifier}" );
			return;
		}

		if ( upgrade.Price > caller.Money )
		{
			Log.Warning( $"{ConsoleSystem.Caller.Name} doesn't have the funds for upgrade {identifier}" );
			return;
		}

		if ( caller.HasUpgrade( identifier ) )
		{
			Log.Warning( $"{ConsoleSystem.Caller.Name} already has upgrade {identifier}" );
			return;
		}

		caller.SetMoney( caller.Money - upgrade.Price );
		caller.AddUpgrade( identifier );
	}
}
