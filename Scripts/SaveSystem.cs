using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// Static class that houses all the methods to save player data
/// as well as chunk data.
/// </summary>
public static class SaveSystem
{
    // file extension for the saved files
    public const string PLAYER_PATH = "/playerData.bin";
    public const string TERRAIN_PATH = "/terrainData.bin";

    /// <summary>
    /// Method that saves the player data. 
    /// </summary>
    /// <param name="player">Player to save</param>
    public static void SavePlayer(Player player)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + PLAYER_PATH;

        FileStream fileStream = new FileStream(path, FileMode.Create);

        PlayerSaveData playerData = new PlayerSaveData(player);

        formatter.Serialize(fileStream, playerData);

        fileStream.Close();
    }

    /// <summary>
    /// Method that reads the player data file and returns a 
    /// playerSaveData class with the information of the player position.
    /// </summary>
    /// <returns>PlayerSaveData Object</returns>
    public static PlayerSaveData LoadPlayer()
    {
        string path = Application.persistentDataPath + PLAYER_PATH;

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fileStream = new FileStream(path, FileMode.Open);

            PlayerSaveData playerData = formatter.Deserialize(fileStream) as PlayerSaveData;


            fileStream.Close();
            return playerData;
        }

        Debug.LogWarning("no player save file at " + path);
        return new PlayerSaveData();
    }

    /// <summary>
    /// Saves the info from the endlessTerrain object to a file
    /// </summary>
    /// <param name="data">endless terrain object to save.</param>
    public static void SaveTerrain(EndlessTerrain data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + TERRAIN_PATH;

        FileStream fileStream = new FileStream(path, FileMode.Create);

        SavedEndlessTerrain savedTerrain = new SavedEndlessTerrain(data);

        formatter.Serialize(fileStream, savedTerrain);

        fileStream.Close();
    }

    /// <summary>
    /// Loads in the saved terrain data from file.
    /// </summary>
    /// <returns>SavedTerrainData object</returns>
    public static SavedEndlessTerrain LoadTerrain()
    {
        string path = Application.persistentDataPath + TERRAIN_PATH;

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fileStream = new FileStream(path, FileMode.Open);

            SavedEndlessTerrain terrainData = formatter.Deserialize(fileStream) as SavedEndlessTerrain;



            fileStream.Close();
            return terrainData;
        }

        Debug.LogWarning("no terrain save file at " + path);
        return new SavedEndlessTerrain();
    }
}
