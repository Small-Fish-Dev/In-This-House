namespace BrickJam;

partial class Player
{
	protected void HandleBodyiew()
	{
		if ( SceneObject is not SceneModel model || Game.LocalPawn != this )
			return;

		var index = GetBoneIndex( "head" );
		var transform = GetBoneTransform( index );

		model.SetBoneWorldTransform( 
			index, 
			transform.WithPosition( transform.Position + Rotation.Backward * 50f ) 
		);
	}
}
