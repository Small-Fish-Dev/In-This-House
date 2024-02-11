using System;
using System.Collections.Generic;
using Sandbox.UI;

namespace Graphicator;

public partial class Actor : Panel
{

#if DEBUG
	private bool _currentlyHandlingModifyEvent;
#endif

	/// <summary> Whether or not the panel allows for Position to be modified </summary>
	private bool _readyToPosition;

	/// <summary> Whether or not panel position should be recalculated </summary>
	private bool _panelNeedsUpdate;

	private readonly Stack<Action<Actor>> _modifyActionStack = new();

	private Vector2? _position;

	/// <summary> Position of actor in panel units </summary>
	public Vector2 Position
	{
		get
		{
#if DEBUG
			if ( !_readyToPosition )
				throw new Exception( "#DEBUG: Position getter used before actor ready" );
#endif
			if ( !_readyToPosition )
				return Vector2.Zero;

			_position ??= CalculateInitialPosition();
			return _position.Value;
		}

		set
		{
#if DEBUG
			if ( !_currentlyHandlingModifyEvent )
				throw new Exception( "#DEBUG: Position setter used outside of Modify() or Act" );
			if ( !_readyToPosition )
				throw new Exception( "#DEBUG: Position setter used before actor ready" );
#endif
			_position = value;
			PositionHasChanged();
		}
	}

	/// <summary> Size of actor in panel units </summary>
	public Vector2 Size => Box.Rect.Size * ScaleFromScreen;

	/// <summary> Actor rect in panel units </summary>
	public Rect Rect => new(Position, Size);

	/// <summary> Calculates actor position from panel position (sbox -> graphicator) </summary>
	/// <returns>Initial position value</returns>
	private Vector2 CalculateInitialPosition() =>
		new()
		{
			x = (Box.Rect.Left - Parent.Box.Rect.Left) * ScaleFromScreen,
			y = (Box.Rect.Top - Parent.Box.Rect.Top) * ScaleFromScreen
		};

	/// <summary> Request a position update </summary>
	public void PositionHasChanged() => _panelNeedsUpdate = true;

	/// <summary> Request an Actor modification </summary>
	/// <param name="action">Action</param>
	public void Modify( Action<Actor> action ) => _modifyActionStack.Push( action );

	/// <summary> Pop & invoke all actions in stack </summary>
	private void ProcessActionStack()
	{
#if DEBUG
		_currentlyHandlingModifyEvent = true;
#endif
		while ( _modifyActionStack.Count > 0 ) _modifyActionStack.Pop().Invoke( this );
#if DEBUG
		_currentlyHandlingModifyEvent = false;
#endif
	}

	public sealed override void Tick()
	{
		base.Tick();

		Style.Position = PositionMode.Absolute;

		if ( _readyToPosition )
		{
			ProcessActionStack();

#if DEBUG
			_currentlyHandlingModifyEvent = true;
#endif
			Act();
#if DEBUG
			_currentlyHandlingModifyEvent = false;
#endif

			if ( _panelNeedsUpdate )
			{
				// Update the panel position
				var position = Position;
				Style.Left = Length.Pixels( position.x );
				Style.Top = Length.Pixels( position.y );
				_panelNeedsUpdate = false;
			}
		}

		if ( _readyToPosition ) return;

		// Basic check for panel readiness
		if ( Box.Rect is { Width: 0, Height: 0 } ) return;

		// We are ready!
		_position ??= CalculateInitialPosition();
		PositionHasChanged();
		_readyToPosition = true;
	}

	protected virtual void Act()
	{
	}
}
