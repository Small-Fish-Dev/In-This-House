using GridAStar;

namespace BrickJam;

public partial class Level
{
	public Grid Grid { get; private set; }
	public virtual BBox WorldBox { get; private set; }

	public async Task GenerateGrid()
	{
		Grid?.Delete();

		var builder = new GridAStar.GridBuilder( Type.ToString() )
			.WithBounds( Vector3.Zero, WorldBox, Rotation.Identity )
			.WithStaticOnly( false )
			.WithoutTags( "door" )
			.WithCellSize( 12f )
			.WithHeightClearance( 72f )
			.WithWidthClearance( 12f )
			.WithStepSize( 24f )
			.WithStandableAngle( 70f )
			.WithMaxDropHeight( 200f );

		Grid = await builder.Create( 1, printInfo: false );

		foreach ( var door in Entity.All.OfType<Door>() )
			if ( Grid.IsInsideBounds( door.Position ) )
				door.OccupyCells();
	}
}
