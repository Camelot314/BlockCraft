using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class used to save information about the player.
/// </summary>
[System.Serializable]
public class PlayerSaveData
{
    private float[] position;

    /// <summary>
    /// Constructor that takes in a player object and saves its position.
    /// </summary>
    /// <param name="player"></param>
    public PlayerSaveData(Player player)
    {
        Vector3 positonVector = player.transform.position;

        position = new float[3];
        position[0] = positonVector.x;
        position[1] = positonVector.y;
        position[2] = positonVector.z;
        
    }

    /// <summary>
    /// Default constructor that sets the position to 0,20,0
    /// </summary>
    public PlayerSaveData()
    {
        position = new float[3];
        position[1] = 20;
    }

    /// <summary>
    /// Returns the positon vector that was saved.
    /// </summary>
    /// <returns>Vector3</returns>
    public Vector3 GetPosVector()
    {
        return new Vector3(position[0], position[1], position[2] + 0.05f);
    }

}
