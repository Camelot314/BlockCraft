using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TreeMapGenerator
{
    public static float[,] GenerateTreeMap(int mapWidth, int mapHeight, int xTrees, int yTrees, int seed, Vector2 offset)
    {
        float[,] map = new float[mapWidth, mapHeight];
        System.Random prng = new System.Random(seed);


        float xDistBet = (float)  mapWidth / xTrees;
        float yDistBet = (float)  mapHeight / yTrees;
        for (int xIndex = 0; xIndex < (xTrees); xIndex++)
        {
            for (int yIndex = 0; yIndex < (yTrees); yIndex++)
            {
                float p = 0.4f;
                float xOff = (float)((prng.NextDouble() * 2*p) - p);
                float yOff = (float)((prng.NextDouble() * 2 * p) - p);
                CreatePoint(new Vector2(offset.x + xDistBet * xIndex + xOff * xDistBet, offset.y + yDistBet * yIndex + yOff * yDistBet), map, prng);
            }
        }
        return map;
    }

    private static void CreatePoint(Vector2 point, float[,] map, System.Random prng)
    {
        int mapSizeX = map.GetLength(0);
        int mapSizeY = map.GetLength(1);

        point.x = Mathf.Clamp(point.x, 1, mapSizeX - 2);                // prevent tree generation on a border
        point.y = Mathf.Clamp(point.y, 1, mapSizeY - 2);

        map[Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y)] = (float)(prng.NextDouble() * 0.75 + .25);  // value between 0.2 and 1

    }
}
