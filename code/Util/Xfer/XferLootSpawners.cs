namespace ITH;

/// <summary>
/// Use this for transferring hammer lootspawners => scene system loot spawner gameobjects.
/// </summary>
public sealed class XferLootSpawners : Component, Component.ExecuteInEditor
{
	protected override void OnEnabled()
	{
		base.OnEnabled();
		Log.Info( "hi" );

		var list = Scene.GetAllObjects( true ).Where( x => x.Name.EqualsOrContains( "lootspawner" ) ).ToList();
		foreach ( var lootspawnerGameObject in list )
		{
			var go = Scene.CreateObject( true );
			go.SetParent( GameObject.Parent );
			go.Name = "LootSpawner";
			go.Components.Create<LootSpawner>();
			go.Transform.Position = lootspawnerGameObject.Transform.Position;
			go.Transform.Rotation = lootspawnerGameObject.Transform.Rotation;
		}
	}
}
