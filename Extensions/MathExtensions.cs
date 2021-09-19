using System;

public static class MathExtensions {
    public static int     Clamp( int val, int min, int max )
    {
        return Math.Min( Math.Max(val, min), max );
    }
}