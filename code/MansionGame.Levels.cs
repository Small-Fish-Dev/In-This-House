namespace ITH;

partial class MansionGame
{
	public Level CurrentLevel;

	[Broadcast]
	public void SetLevel( LevelType levelId )
	{
		SetLevelAsync( levelId );
	}

	private async Task SetLevelAsync( LevelType id )
	{
		if ( CurrentLevel is not null )
			await CurrentLevel.End();

		CurrentLevel = Levels.First( x => x.Id == id );
		await CurrentLevel.Start();
		Level.OnLevelChanged?.Invoke( CurrentLevel );

		await Task.CompletedTask;
	}
}
