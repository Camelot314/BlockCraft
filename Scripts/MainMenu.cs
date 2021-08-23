using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script for the buttons on the main menu
/// </summary>
public class MainMenu : MonoBehaviour
{
    #region Fields
    [SerializeField] private bool webMode;
    #endregion

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
    /// It will not quit the game if the game is in web Mode
    /// </summary>
    public void QuitGame()
    {
        if (webMode)
        {
            return;
        }
        Debug.Log("Quitting");
        Application.Quit();
    }
    #endregion


}
