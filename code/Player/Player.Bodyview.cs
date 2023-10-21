namespace BrickJam;

partial class Player
{
	public static void HandleBodyview( AnimatedEntity entity )
	{
		var target = Camera.FirstPersonViewer as AnimatedEntity;
		if ( target?.SceneObject is not SceneModel model )
			return;

		var index = entity.GetBoneIndex( "head" );
		var transform = entity.GetBoneTransform( index );

		model.SetBoneWorldTransform( 
			index, 
			transform.WithPosition( transform.Position + entity.Rotation.Backward * 50f ) 
		);
	}
}
