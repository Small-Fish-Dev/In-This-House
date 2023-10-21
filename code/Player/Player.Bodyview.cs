namespace BrickJam;

partial class Player
{
	public static void HandleBodyview()
	{
		var target = Camera.FirstPersonViewer as AnimatedEntity;
		if ( target?.SceneObject is not SceneModel model )
			return;

		var index = target.GetBoneIndex( "head" );
		var transform = target.GetBoneTransform( index );

		model.SetBoneWorldTransform( 
			index, 
			transform.WithPosition( transform.Position + target.Rotation.Backward * 50f ) 
		);
	}
}
