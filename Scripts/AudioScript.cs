using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioScript : MonoBehaviour
{
    #region Fields
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClips;

    private int currentTrack;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        audioSource.volume = 0.5f;
        audioSource.clip = audioClips[0];
        audioSource.Play();
        Invoke("NextTrack", audioClips[0].length);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    #endregion


    #region Private Methods
    /// <summary>
    /// Changes the track to the next one in the array.
    /// Loops around when over.
    /// </summary>
    private void NextTrack()
    {
        if (currentTrack == audioClips.Length - 1)
        {
            currentTrack = 0;
        } else
        {
            currentTrack++;
        }
        audioSource.clip = audioClips[currentTrack];
        audioSource.Play();
        Invoke("NextTrack", audioClips[currentTrack].length);
    }

    #endregion

    #region Public Methods



    #endregion
}
