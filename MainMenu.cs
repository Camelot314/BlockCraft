using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script for the buttons on the main menu
/// </summary>
public class MainMenu : MonoBehaviour
{
    #region Public Methods
    /// <summary>
    /// Plays the game from the main menu.
    /// </summary>
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }


    /// <summary>
    /// Quits the game from the menu.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting");
        Application.Quit();
    }
    #endregion


}
