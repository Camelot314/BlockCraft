using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavingHandler : MonoBehaviour
{
    #region Fields
    [SerializeField] private GameObject player;
    [SerializeField] private EndlessTerrain terrainHolder;

    private Player playerScript;
    private CharacterController playerController;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        playerScript = player.GetComponent<Player>();
        playerController = player.GetComponent<CharacterController>();
    }
    #endregion

    #region Public Methods
    
    /// <summary>
    /// Saves the player location and all the edited chunks.
    /// </summary>
    public void Save()
    {
        SavePlayer();
        SaveTerrain();
        Debug.Log("Quitting");
        Application.Quit();
    }

    /// <summary>
    /// Saves the terrainData to a file.
    /// </summary>
    private void SaveTerrain()
    {
        SaveSystem.SaveTerrain(terrainHolder);
    }

    /// <summary>
    /// Returns the saved information for the terrain
    /// </summary>
    /// <returns>SavedEndlessTerrain object</returns>
    public SavedEndlessTerrain LoadTerrain()
    {
        return SaveSystem.LoadTerrain();
    }

    /// <summary>
    /// Saves the player to a file.
    /// </summary>
    private void SavePlayer()
    {
        SaveSystem.SavePlayer(playerScript);
    }



    /// <summary>
    /// Loads the player data and sets the position of the 
    /// player to that which was found in the file.
    /// </summary>
    public void LoadPlayer()
    {
        PlayerSaveData playerData = SaveSystem.LoadPlayer();
        playerController.enabled = false;
        playerController.transform.position = playerData.GetPosVector();
        playerController.enabled = true;
    }
    #endregion
}
