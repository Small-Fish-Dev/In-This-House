using Sandbox;

namespace ITH.UI;

public partial class GraphicActor
{
	/// <summary> Position the actor over a point in world space </summary>
	/// <param name="point">Point in world</param>
	/// <param name="center">Should the actor be centered over the point?</param>
	public void MoveToWorldPoint( Vector3 point, bool center = true )
	{
		var x = (Vector2)point.ToScreen();

		// ToScreen() returns 0-1, so multiply by screen size
		x *= Screen.Size;

		// We have the current screen position of the point
		// To center it, just subtract half the size of the actor (Box.Rect is panel size on screen)
		if ( center )
			x -= Box.Rect.Size * 0.5f;

		// Actor.Position uses panel units - convert from screen units
		x *= ScaleFromScreen;

		Position = x;
	}
}
