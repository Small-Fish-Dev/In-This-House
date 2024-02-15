namespace ITH;

/// <summary>
/// Use this for transferring hammer doors => scene doors
/// </summary>
public sealed class XferDoors : Component, Component.ExecuteInEditor
{
	[Property] public GameObject DoorPrefab { get; set; }
	[Property] public bool SpawnerEnabled = false;

	protected override void OnEnabled()
	{
		base.OnEnabled();
		if ( !SpawnerEnabled )
			return;

		Log.Info( "Door Xfer started..." );

		var list = Scene.GetAllObjects( true ).Where( x => x.Name.EqualsOrContains( "door" ) ).ToList();
		Log.Info( $"Doors: {list.Count}" );

		foreach ( var door in list )
		{
			var go = DoorPrefab.Clone( GameObject.Parent, door.Transform.Position, door.Transform.Rotation, Vector3.One );
			go.Name = "Door";
		}
	}
}
