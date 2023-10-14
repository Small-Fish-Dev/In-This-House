using GridAStar;
using System.Collections.Generic;
using System.Threading;

namespace BrickJam;

public partial class NPC
{
	private AStarPath currentPath { get; set; }
	public AStarPath CurrentPath
	{
		get => currentPath;
		set
		{
			if ( !currentPath.IsEmpty && currentPath.Nodes == value.Nodes )
				return;
			currentPath = value;
			HasArrivedDestination = false;
		}
	}
	public Grid CurrentGrid => MansionGame.Instance.CurrentLevel.Grid;
	public virtual float PathRetraceFrequency { get; set; } = 0.5f; // How many seconds before it checks if the path is being followed or the target position changed
	internal CancellationTokenSource CurrentPathToken { get; set; } = new();
	public AStarNode CurrentPathNode => IsFollowingPath ? CurrentPath.Nodes[0] : null; // The latest cell crossed in the path
	public AStarNode LastPathNode => IsFollowingPath ? CurrentPath.Nodes[^1] : null; // The final cell in the path
	public AStarNode NextPathNode => IsFollowingPath ? CurrentPath.Nodes[Math.Min( 1, CurrentPath.Count - 1 )] : null;
	public string NextMovementTag => IsFollowingPath ? NextPathNode.MovementTag : string.Empty;
	public bool IsFollowingPath => !HasArrivedDestination && CurrentPath.Count > 0; // Is the entity following a path
	private Line CurrentPathLine => new( CurrentPathNode.EndPosition, NextPathNode.EndPosition );
	public float DistanceFromIdealPath => CurrentPathLine.Distance( Position ); // How far the entity strayed off path, used to recalculate path
	public Entity Target { get; set; } = null;
	public Vector3 IdealDirection => IsFollowingPath ? (NextPathNode.EndPosition.WithZ( 0 ) - Position.WithZ( 0 )).Normal : Vector3.Zero;
	public bool IsFollowingSomeone => IsFollowingPath && Target != null; // Is the entity following a moving target
	public bool HasArrivedDestination { get; private set; } = true; // Has the entity successfully reached their destination
	public bool ForcedStop { get; set; } = false;
	internal TimeUntil NextRetraceCheck { get; set; } = 0f;
	
	public void ComputeNavigation()
	{
		if ( Game.IsClient ) return;
		if ( CurrentGrid == null ) return;

		if ( NextRetraceCheck )
		{
			var targetPathCell = GetTargetPathCell();

			if ( targetPathCell != null )
			{
				if ( IsFollowingPath )
				{
					if ( targetPathCell != LastPathNode.Current ) // If the target cell is not the current path's last cell, retrace path
						NavigateTo( targetPathCell );
					else
					{
						if ( GroundEntity != null ) // This code is useless when they're jumping or dropping
						{
							var minimumDistanceUntilRetrace = CurrentGrid.CellSize * 1.42f + CurrentGrid.StepSize / 2f;

							if ( IsFollowingPath && DistanceFromIdealPath > minimumDistanceUntilRetrace ) // Or if you strayed away from the path too far
								NavigateTo( targetPathCell );
						}
					}
				}
				else
					NavigateTo( targetPathCell );
			}

			NextRetraceCheck = PathRetraceFrequency;
		}

		if ( IsFollowingPath )
		{
			for ( var i = 1; i < CurrentPath.Count; i++ )
			{
				//DebugOverlay.Line( CurrentPath.Nodes[i-1].EndPosition, CurrentPath.Nodes[i].EndPosition, Time.Delta, false );
				//DebugOverlay.Text( CurrentPath.Nodes[i-1].MovementTag, CurrentPath.Nodes[i-1].EndPosition, Time.Delta, 5000f );
			}

			Direction = IdealDirection;

			var minimumDistanceUntilNext = CurrentGrid.CellSize; // * 1.42f ?

			if ( Position.WithZ( 0 ).Distance( NextPathNode.EndPosition.WithZ( 0 ) ) <= minimumDistanceUntilNext ) // Move onto the next cell
				if ( Math.Abs( Position.z - NextPathNode.EndPosition.z ) <= CurrentGrid.StepSize ) // Make sure it's within the stepsize
				{
					CurrentPath.Nodes.RemoveAt( 0 );

					if ( NextMovementTag == "shortjump" )
					{
						Position = CurrentPathNode.EndPosition;
						Direction = (NextPathNode.EndPosition.WithZ(0) - Position.WithZ(0)).Normal;
						Velocity = (Direction * 200f).WithZ( 300f );
						SetAnimParameter( "jump", true );
						GroundEntity = null;
					}

					if ( CurrentPathNode == LastPathNode )
						HasArrivedDestination = true;
				}
		}
	}

	public Cell GetTargetPathCell()
	{
		if ( Target != null )
			return CurrentGrid.GetCell( Target.Position ) ?? CurrentGrid.GetNearestCell( Target.Position );
		else if ( IsFollowingPath )
			return LastPathNode.Current;
		else
			return null;
	}

	public void NavigateTo( Cell targetCell )
	{
		GameTask.RunInThreadAsync( async () =>
		{
			var startingCell = CurrentGrid.GetCell( Position ) ?? CurrentGrid.GetNearestCell( Position );

			if ( startingCell == null || targetCell == null || startingCell == targetCell ) return;

			var pathBuilder = new AStarPathBuilder( CurrentGrid )
			.WithPathCreator( this );

			if ( false && CurrentGrid.LineOfSight( startingCell, targetCell ) ) // If there's direct line of sight, move in a straight path from A to B
			{
				var nodeList = new List<AStarNode>() { new AStarNode( startingCell ), new AStarNode( targetCell ) };
				CurrentPath = AStarPath.From( pathBuilder, nodeList );
			}
			else
			{
				CurrentPathToken.Cancel();
				CurrentPathToken = new CancellationTokenSource();

				var computedPath = await pathBuilder.RunAsync( startingCell, targetCell, CurrentPathToken.Token );

				if ( computedPath.IsEmpty || computedPath.Length < 1 )
					return;

				computedPath.Simplify();

				CurrentPath = computedPath;
			}
		} );
	}
}
