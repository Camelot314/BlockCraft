using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class VolumeSlider : MonoBehaviour
{
    #region Fields
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Slider volumeSlider;
    #endregion

    #region Unity Methods

    /// <summary>
    /// Sets the volume slider value to the volume of the audioSource
    /// </summary>
    private void Awake()
    {
        volumeSlider.normalizedValue = audioSource.volume;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Sets the volume of the audio source to the value of the slider.
    /// </summary>
    public void ChangeVolume()
    {
        audioSource.volume = volumeSlider.normalizedValue;
    }
    #endregion
}
