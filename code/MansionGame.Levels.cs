namespace ITH;

partial class MansionGame
{
	public Level CurrentLevel;

	[Broadcast]
	public void SetLevel( LevelType levelId )
	{
		CurrentLevel = Levels.First( x => x.Id == levelId );
		Level.OnLevelChanged?.Invoke( CurrentLevel );

		if ( Networking.IsHost )
		{
			foreach ( var client in Clients.Values )
			{
				if ( client.Pawn.Components.TryGet<PlayerController>( out var player ) )
				{
					var sp = Random.Shared.FromList( CurrentLevel.Spawns );
					player.Respawn( sp.Transform.Position );
				}
			}
		}
	}
}
