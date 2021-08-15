using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioScript : MonoBehaviour
{
    #region Fields
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClips;

    private int currentTrack;
    private System.Random random;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        random = new System.Random();
        audioSource.volume = 0.5f;
        int clipNumber = random.Next(0, audioClips.Length);
        audioSource.clip = audioClips[clipNumber];
        audioSource.Play();
        Invoke("NextTrack", clipNumber);
    }

    #endregion


    #region Private Methods
    /// <summary>
    /// Changes the track to the next one in the array.
    /// Loops around when over.
    /// </summary>
    private void NextTrack()
    {
        currentTrack = random.Next(0, audioClips.Length);
        audioSource.clip = audioClips[currentTrack];
        audioSource.Play();
        Invoke("NextTrack", audioClips[currentTrack].length);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Pauses the audio source and prevents the next track from playing too soon.
    /// </summary>
    /// <param name="shouldPause"></param>
    public void Pause(bool shouldPause)
    {
        if (shouldPause)
        {
            audioSource.Pause();
            CancelInvoke();
        } else
        {
            audioSource.Play();
            float time = audioSource.clip.length - audioSource.time;
            time = time < 0 ? 0 : time;
            Invoke("NextTrack", time);
        }

    }

    #endregion
}
