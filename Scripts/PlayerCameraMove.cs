using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraMove : MonoBehaviour
{
    #region Fields
    [SerializeField] private GameObject player;
    [SerializeField] private bool fps;
    [SerializeField] private float mouseSensitivity = 100;

    private Player brain;
    private Transform playerTransform;
    private float xRotation = 0;
    #endregion

    #region Unity Methods
    /// <summary>
    /// Gets the references and locks the cursor.
    /// </summary>
    private void Awake()
    {
        brain = player.GetComponent<Player>();
        playerTransform = player.transform;
        fps = brain.GetCamType() == Player.CamType.FirstPerson;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// If FPS is active then it will move the camera according to the
    /// logic in MoveCam;
    /// </summary>
    void FixedUpdate()
    {
        if (fps)
        {
            MoveCam();
        }
    }

    #endregion

    #region Public Methods
    /// <summary>
    /// Sets the FPS bool
    /// </summary>
    /// <param name="fps">parameter to change fps to</param>
    public void SetFPS(bool fps)
    {
        this.fps = fps;
    }

    /// <summary>
    /// Getter for the fps state of the object. 
    /// </summary>
    /// <returns>true if it is fps</returns>
    public bool IsFPS()
    {
        return fps;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Moves the camera based on the input for the x and y locations. 
    /// </summary>
    private void MoveCam()
    {
        float mouseX, mouseY;

        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        playerTransform.Rotate(Vector3.up * mouseX);


    }
    #endregion
}
