﻿namespace BrickJam;

public static class MathE
{

	public static float SmoothStep( float frac, bool clamp = true )
	{
		if ( clamp )
		{
			frac = frac.Clamp( 0f, 1f );
		}

		frac = MathX.Lerp( (float)-Math.PI / 2, (float)Math.PI / 2, frac );
		frac = (float)Math.Sin( frac );
		frac = (frac / 2f) + 0.5f;

		return frac;
	}

	public static float Slerp( float from, float to, float frac, bool clamp = true )
	{
		frac = SmoothStep( frac, clamp );

		return from + frac * (to - from);

	}

	public static float SmoothKernel( float radius, float distance )
	{
		var value = Math.Max( 0, radius * radius - distance * distance );
		value = MathX.Remap( value, 0f, radius * radius, 0f, 1f );
		return value * value;
	}

}
