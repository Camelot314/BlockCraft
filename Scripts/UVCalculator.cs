using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UVCalculator
{
    public const int TILE_WIDTH = 32, TILE_HEIGHT = 16;
    public static bool displayed = false;


    public static Rect GetUVs(BlockType type, BlockSide side)
    {
        return GetUVs(type.GetCoords(side));
    }

    #region Private methods
    /// <summary>
    /// The values are x = u min, y = u max, z = v min, w = z max
    /// </summary>
    /// <param name="tileLocation">Coordinate on atlas</param>
    /// <returns>Rect the the information needed to make the uv for the block face</returns>
    private static Rect GetUVs(Vector2Int tileLocation)
    {

        Rect values = new Rect();

        float tilePercentX = 1f / TILE_WIDTH;
        float tilePercentY = 1f / TILE_HEIGHT;


        values.x = tilePercentX * (tileLocation.x);
        values.y = tilePercentY * (tileLocation.y);

        values.width = tilePercentX;
        values.height = tilePercentY;

        return values;
    }
    #endregion
}
