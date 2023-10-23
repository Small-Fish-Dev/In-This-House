using BrickJam.Upgrading;
using Sandbox.UI;

namespace BrickJam.UI;

public partial class Shop : Panel
{
	private List<Upgrade> PossibleUpgrades { get; set; } = new();
	private List<Upgrade> LockedUpgrades { get; set; } = new();

	public Shop()
	{
		instance = this;

		UpdateUpgradeList();
	}

	[Event( "UpgradeBought" )]
	private void OnUpgradeBought(string identifier)
	{
		if ( Game.IsServer )
			return;

		UpdateUpgradeList();
		
		Log.Info( $"OnUpgradeBought: {identifier}" );
	}

	private bool CanBuyUpgrade( Upgrade upgrade )
	{
		if ( Game.LocalPawn is not Player player )
			return false;

		if ( player.HasUpgrade( upgrade.Identifier ) )
			return false;

		if ( upgrade.Price > player.Money )
			return false;

		if ( upgrade.Dependencies.Any( upgradeDependency => !player.HasUpgrade( upgrade.Identifier ) ) )
			return false;

		return true;
	}

	private void UpdateUpgradeList()
	{
		PossibleUpgrades.Clear();
		LockedUpgrades.Clear();

		if ( Game.LocalPawn is not Player player )
			return;

		foreach ( var upgrade in Upgrade.All )
		{
			Log.Info( upgrade );
			if ( CanBuyUpgrade( upgrade ) )
				PossibleUpgrades.Add( upgrade );
			else
				LockedUpgrades.Add( upgrade );
		}
		
		StateHasChanged();
	}

	private void BuyUpgrade( Upgrade upgrade )
	{
		if ( Game.LocalPawn is not Player player )
			return;

		if ( !CanBuyUpgrade( upgrade ) )
		{
			// This shouldn't be possible - let's be careful though
			Log.Warning( "BuyUpgrade tried to buy weird upgrade!!!" );
			return;
		}
		
		Player.BuyUpgrade( upgrade.Identifier );
	}
}
