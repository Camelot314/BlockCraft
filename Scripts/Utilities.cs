using UnityEngine;

public static class Utilities
{
    /// <summary>
    /// Returns true if the two float values are within 0.001 of each other.
    /// </summary>
    /// <param name="a">first float</param>
    /// <param name="b">second float</param>
    /// <returns>bool true if equal</returns>
    public static bool Equal(float a, float b)
    {
        return Mathf.Abs(a - b) < 0.001;
    }

    /// <summary>
    /// Returns true if the the two vectors have values within 0.001 of each other.
    /// </summary>
    /// <param name="a">first vector</param>
    /// <param name="b">second vector</param>
    /// <returns>bool true if equal</returns>
    public static bool Equal(Vector3 a, Vector3 b)
    {
        bool equal = true;

        equal = equal && Equal(a.x, b.x);
        equal = equal && Equal(a.y, b.y);
        equal = equal && Equal(a.z, b.z);

        return equal;
    }
    
    /// <summary>
    /// Returns true if the the two vectors have values that are equal.
    /// </summary>
    /// <param name="a">First vector</param>
    /// <param name="b">Second vector</param>
    /// <returns>bool true if equal</returns>
    public static bool Equal(Vector3Int a, Vector3Int b)
    {
        bool equal = true;

        equal = equal && a.x == b.x;
        equal = equal && a.y == b.y;
        equal = equal && a.z == b.z;

        return equal;
    }

}