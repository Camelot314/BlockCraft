using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    #region Public enumerator
    public enum CamType { FirstPerson, ThirdPerson }
    #endregion

    #region Instance Variables
    [Header("References")]
    [SerializeField] private SavingHandler saver;
    [SerializeField] private EndlessTerrain endlessTerrainScript;
    [SerializeField] private Transform eyes;
    [SerializeField] private ItemBar itemBar;
    [SerializeField] private GameObject hat, crossHairs, hud, pauseMenu;
    [SerializeField] private AudioScript audioScript;

    [Header("Camera Controls")]
    [SerializeField] private CamType camMode;
    [SerializeField] private GameObject tpsCam, mainCam;

    [Header("Player Controls")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private bool flightPossible;
    [SerializeField] [Range(1, 1000)] private long doublePressPeriod = 300;
    [SerializeField]
    private float
        gravity = -9.81f,
        moveSpeedMult = 10f,
        flightVel = 2,
        jumpHeight = 3,
        turnSmoothTime = 0.1f,
        groundSphere = 0.4f;

    [Header("Physics Interaction Vars")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector3 tpsOffset, fpsOffset;


    [Header("CrossHair Controls")]
    [SerializeField] private Color highlightedColor = Color.green;
    [SerializeField] private Color normalColor = Color.grey;

    private PlayerCameraMove fpsCamScript;
    private CinemachineBrain tpsCamBrain;
    private Camera cameraSettings;
    private TextMeshProUGUI crossHairsTextMesh;
    private Color prevColor;
    private Vector3 velocity = Vector3.zero;
    private bool
        grounded, flying, firstJump,
        leftClick, rightClick, hideHUD, paused;
    private float jumpVel, turnSmoothVel;
    private long lastJump;
    private short blockType = 2;

    #endregion

    #region Public Methods
    
    /// <summary>
    /// this will pause everything if pause is true;
    /// It will also toggle the hud. 
    /// </summary>
    /// <param name="pause"></param>
    public void Pause(bool pause)
    {
        this.paused = pause;
        ToggleHUD(pause);
        flying = pause;
        pauseMenu.SetActive(pause);
        audioScript.Pause(pause);
        hideHUD = pause;

        if (camMode == CamType.FirstPerson)
        {
            fpsCamScript.SetFPS(!pause);
        } else
        {
            tpsCamBrain.enabled = !pause;
        }

        if (pause)
        {
            Cursor.lockState = CursorLockMode.None;

        } else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }


    /// <summary>
    /// Gets the current camera type for the player.
    /// </summary>
    /// <returns>An enumerator of the current camera type.</returns>
    public CamType GetCamType()
    {
        return camMode;
    }

    /// <summary>
    /// Sets the camera type to the one provided.
    /// </summary>
    /// <param name="camMode">Enumerator Camera type</param>
    public void SetCamType(CamType camMode)
    {
        this.camMode = camMode;
        ChangeCamera(camMode);
    }

    #endregion


    #region Unity Methods

    /// <summary>
    /// This will make sure that the inspector value for gravity is 
    /// always less than or equal to zero. Also makes sure that the 
    /// jump height is always greater than or equal to zero. 
    /// </summary>
    private void OnValidate()
    {
        if (!Utilities.Equal(gravity, 0) && gravity > 0)
        {
            gravity *= -1;
        }
        if (!Utilities.Equal(jumpHeight, 0) && jumpHeight < 0)
        {
            jumpHeight *= -1;
        }

    }

    /// <summary>
    /// Calculates the jump velocity so that it will not
    /// have to do it every time jump is pressed. 
    /// It also gets the scripts for the camera brain and script.
    /// </summary>
    private void Start()
    {
        jumpVel = Mathf.Sqrt(jumpHeight * -2 * gravity);
        cameraSettings = mainCam.GetComponent<Camera>();
        fpsCamScript = mainCam.GetComponent<PlayerCameraMove>();
        tpsCamBrain = mainCam.GetComponent<CinemachineBrain>();
        crossHairsTextMesh = crossHairs.GetComponent<TextMeshProUGUI>();
        prevColor = normalColor;
        crossHairsTextMesh.color = normalColor;
        ChangeCamera(camMode);
        hud.SetActive(!hideHUD);
        saver.LoadPlayer();

    }

    /// <summary>
    /// This will move the character every frame.
    /// 
    /// This also checks to see if the camera needs to be swtiched.
    /// 
    /// It finally edits the block the player is looking at if necessary.
    /// </summary>
    private void Update()
    {
        SetPause();


        if (paused)
        {
            return;
        }

        MovePlayer();
        SelectCamera();
        ChangeBlock();
        EditBlock();
        if (Input.GetKeyDown(KeyCode.F3))
        {
            hideHUD = !hideHUD;
            ToggleHUD(!hideHUD);
        }


    }




    #endregion

    #region Private Methods

    /// <summary>
    /// This enables or disables the HUD for the player.
    /// </summary>
    /// <param name="hide">true to disable the HUD</param>
    private void ToggleHUD(bool hide)
    {
        hud.SetActive(!hide);
        prevColor = normalColor;
    }

    /// <summary>
    /// this will pause the player when the escape key is pressed.
    /// </summary>
    private void SetPause()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause(!paused);
        }
    }


    /// <summary>
    /// Changes the blockType of the player depending on the input from the numbers 
    /// on the keyboard. It also changes the block according to the scroll.
    /// </summary>
    private void ChangeBlock()
    {
        float mouseScrollY = Input.mouseScrollDelta.y;
        if (Utilities.Equal(mouseScrollY, 0))
        {
            blockType = Input.GetKeyDown(KeyCode.Alpha1) ? (short)2 : blockType;
            blockType = Input.GetKeyDown(KeyCode.Alpha2) ? (short)3 : blockType;
            blockType = Input.GetKeyDown(KeyCode.Alpha3) ? (short)4 : blockType;
            blockType = Input.GetKeyDown(KeyCode.Alpha4) ? (short)5 : blockType;
            blockType = Input.GetKeyDown(KeyCode.Alpha5) ? (short)6 : blockType;
            blockType = Input.GetKeyDown(KeyCode.Alpha6) ? (short)7 : blockType;
            blockType = Input.GetKeyDown(KeyCode.Alpha7) ? (short)8 : blockType;
            blockType = Input.GetKeyDown(KeyCode.Alpha8) ? (short)9 : blockType;
            blockType = Input.GetKeyDown(KeyCode.Alpha9) ? (short)10 : blockType;
            blockType = Input.GetKeyDown(KeyCode.Alpha0) ? (short)11 : blockType;
        } else
        {
            if (mouseScrollY > 0)
            {
                blockType++;
                blockType = blockType > 11 ? (short)2 : blockType;
            }

            if (mouseScrollY < 0)
            {
                blockType--;
                blockType = blockType < 2 ? (short)11 : blockType;
            }
        }
        

        if (!hideHUD)
        {
            itemBar.ChangeHighlighted(blockType);
        }
    }

    /// <summary>
    /// If tab is pressed then the camera will switch from third person
    /// perspective to first person perspective. This doesn't change the camera
    /// just the location of the camera and how it behaves. 
    /// </summary>
    private void SelectCamera()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (camMode == CamType.ThirdPerson)
            {
                SetCamType(CamType.FirstPerson);
            }
            else
            {
                SetCamType(CamType.ThirdPerson);
            }
        }
    }


    /// <summary>
    /// This will move the player using the cursor location, 
    /// the wasd keys and space. 
    /// </summary>
    private void MovePlayer()
    {
        Vector3 moveSpeed = AddGravityAndJump();

        switch (camMode)
        {
            case CamType.FirstPerson:
                moveSpeed = FPSMovement(moveSpeed);
                break;
            case CamType.ThirdPerson:
                moveSpeed = TPSMovement(moveSpeed);
                break;
        }
        if (grounded && moveSpeed.y < 0)
        {
            moveSpeed.y = 0;
        }
        if (moveSpeed != Vector3.zero)
        {
            controller.Move(moveSpeed * Time.deltaTime);
        }
    }


    /// <summary>
    /// This method checks to see if the player is looking at a block that is within 
    /// range. If the block is within range then it will either add a block or
    /// remove a block depending on the click.
    /// 
    /// You cannot create and edit blocks when the perspective is third person.
    /// </summary>
    private void EditBlock()
    {
        if (camMode == CamType.ThirdPerson)
        {
            return;
        }
        Ray ray = new Ray(eyes.position, mainCam.transform.forward);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, 4))
        {
            Debug.DrawLine(ray.origin, hitInfo.point, Color.red);

            RemoveBlock(ray, hitInfo);
            AddBlock(ray, hitInfo);
            ChangeCrossHair(true);

        }
        else
        {
            ChangeCrossHair(false);
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * 4, Color.green);        // Line is not visible in finished build of game.
        }

    }

    /// <summary>
    /// Draws the corssHair color if hideHUD is false. The parameter indicates 
    /// whether to draw it the highlighted color. 
    /// 
    /// If hideHUD is true then it does nothing.
    /// </summary>
    /// <param name="highlight">true if crossH</param>
    private void ChangeCrossHair(bool highlight)
    {

        if (hideHUD)
        {
            return;
        }

        if (highlight)
        {
            if (prevColor.Equals(normalColor))
            {
                prevColor = highlightedColor;
                crossHairsTextMesh.color = highlightedColor;
            }
        } else
        {
            if (prevColor.Equals(highlightedColor))
            {
                prevColor = normalColor;
                crossHairsTextMesh.color = normalColor;
            }
        }

    }

    /// <summary>
    /// This will add a block if the right click is pressed (only once per press)
    /// at the specified location.
    /// </summary>
    /// <param name="ray">Ray to use for determining camera direction</param>
    /// <param name="hitInfo">Information about the object the ray hit.</param>
    private void AddBlock(Ray ray, RaycastHit hitInfo)
    {
        rightClick = Input.GetMouseButtonDown(1);
        if (rightClick)
        {
            AdjustChunk(ray.direction, hitInfo, true);
        }
    }

    /// <summary>
    /// This will remove a block if the left click is pressed (only once per press)
    /// at the specified location. 
    /// </summary>
    /// <param name="ray">Ray to use for determining camera direction</param>
    /// <param name="hitInfo">Information about the object the ray hit.</param>
    private void RemoveBlock(Ray ray, RaycastHit hitInfo)
    {
        leftClick = Input.GetMouseButtonDown(0);
        if (leftClick)
        {
            AdjustChunk(ray.direction, hitInfo, false);
        }
    }

    /// <summary>
    /// This adjusts the terrain chunk that the player selected. Either adding a
    /// block or removing a block from the location that the player is looking at.
    /// 
    /// It will first get the chunk position from the hit info. It will use that to
    /// grab the script associated with the location from the Endless terrain class.
    /// 
    /// It will then use that object ot edit the cube. 
    /// </summary>
    /// <param name="rayDirection">Vector3 direction the player is looking</param>
    /// <param name="hitInfo">RaycastHit with info about the object hit</param>
    /// <param name="addBlock">wheter to add a block.</param>
    private void AdjustChunk(Vector3 rayDirection, RaycastHit hitInfo, bool addBlock)
    {
        Vector3 chunkPos = hitInfo.transform.position;
        TerrainChunk chunk = endlessTerrainScript.GetChunkFromCenter(chunkPos);

        TerrainChunk.OutOfBoundsType overLoad = chunk.EditCube(
            rayDirection, hitInfo.point, 
            groundCheck.position, addBlock, blockType
            );
        Vector3 adjustedPoint = Vector3.zero;
        bool changed = false;
        switch (overLoad)
        {
            case TerrainChunk.OutOfBoundsType.PosX:
                adjustedPoint = hitInfo.transform.position + new Vector3(MapGenerator.GetMapVerts() / 2f + 1, 0 , 0);
                changed = true;
                break;
            case TerrainChunk.OutOfBoundsType.NegX:
                adjustedPoint = hitInfo.transform.position + new Vector3(-MapGenerator.GetMapVerts() / 2f - 1, 0, 0);
                changed = true; 
                break;
            case TerrainChunk.OutOfBoundsType.PosZ:
                adjustedPoint = hitInfo.transform.position + new Vector3(0, 0, MapGenerator.GetMapVerts() / 2f + 1);
                changed = true;
                break;
            case TerrainChunk.OutOfBoundsType.NegZ:
                adjustedPoint = hitInfo.transform.position + new Vector3(0, 0, - MapGenerator.GetMapVerts() / 2f - 1);
                changed = true;
                break;
            default:
                changed = false;
                break;
        }
        if (changed)
        {
            chunk = endlessTerrainScript.GetChunkFromCenter(adjustedPoint);
            
            chunk.EditCube(
            rayDirection, hitInfo.point,
            groundCheck.position, addBlock, blockType
            );

        }
    }

    /// <summary>
    /// This will change the camera type to the given one.
    /// If the camType is first person then it will set the local
    /// position of the camera to the fps camera offset.
    /// 
    /// It will then make sdure that the cinemachineBrain for the 
    /// main brain is dissabled as well as the tpsCam.
    /// 
    /// If the type is Third person then it will set the local
    /// position to the offset and enable all the Cinemachine features. 
    /// 
    /// THIS METHOD DOES NOT CHANGE THE CAM MODE OF THIS OBJECT
    /// </summary>
    /// <param name="type">Camera type to change to</param>
    private void ChangeCamera(CamType type)
    {
        if (type == CamType.FirstPerson)
        {
            mainCam.transform.localPosition = fpsOffset;
            cameraSettings.fieldOfView = 85;
            tpsCamBrain.enabled = false;
            fpsCamScript.SetFPS(true);
            tpsCam.SetActive(false);
            hat.SetActive(false);
            crossHairs.SetActive(true);
            

        }
        else
        {
            mainCam.transform.localPosition = tpsOffset;
            tpsCamBrain.enabled = true;
            fpsCamScript.SetFPS(false);
            tpsCam.SetActive(true);
            hat.SetActive(true);
            crossHairs.SetActive(false);
        }

    }



    /// <summary>
    /// the method that calculates the movement vector 
    /// for when the player is in third person perspective mode.
    /// It also does the player rotation.
    /// </summary>
    /// <param name="moveMent">The movement vector that is already loaded with gravity and jump.</param>
    /// <returns>new velocity vector for the entire movement</returns>
    private Vector3 TPSMovement(Vector3 moveMent)
    {
        float x, z;
        PlayerVelocity(out x, out z);

        Vector3 direction = new Vector3(x, 0, z).normalized;
        Vector3 moveDir = Vector3.zero;

        if (direction.sqrMagnitude >= 0.01f)
        {
            float targetAngle =
                Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg
                + mainCam.transform.eulerAngles.y;

            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref turnSmoothVel,
                turnSmoothTime
            );

            transform.rotation = Quaternion.Euler(0, angle, 0);
            moveDir = (Quaternion.Euler(0, targetAngle, 0) * Vector3.forward).normalized;
        }


        return moveMent + moveDir * CalcMoveSpeed();
    }

    /// <summary>
    /// This sets the player velocity to those specified by the wasd or up down left right keys.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    private void PlayerVelocity(out float x, out float z)
    {
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");
    }

    /// <summary>
    /// The method that calculates the movement vector 
    /// for when the player is in first person perspective mode
    /// </summary>
    /// <param name="movement">The movement vector that is already loaded with gravity and jump</param>
    /// <returns>new velocity vector for entire movement.</returns>
    private Vector3 FPSMovement(Vector3 movement)
    {
        float x, z;
        PlayerVelocity(out x, out z);
        movement += ((transform.right * x + transform.forward * z).normalized * CalcMoveSpeed());
        return movement;
    }

    /// <summary>
    /// Calculates the movement speed using the speed multiplier. It is different if the player is flying.
    /// </summary>
    /// <returns>float the velocity at which to move</returns>
    private float CalcMoveSpeed()
    {
        float moveSpeed = moveSpeedMult;
        if (flying)
        {
            moveSpeed *= (1 + Evaluate(transform.position.y) / 2f);
        }

        return moveSpeed;
    }

    /// <summary>
    /// This clamps the input float over 100 to a value
    /// between to and 30.
    /// </summary>
    /// <param name="x">input float</param>
    /// <returns>value between 10 and 30</returns>
    private float Evaluate(float x)
    {
        return Mathf.Clamp(x / 100, 10, 30);
    }

    /// <summary>
    /// Method that adds the velocity for gravity as well as jump.
    /// The character will only jump if the character is grounded.
    /// </summary>
    /// <returns>Vector3 where the y value is the velocity due to gravity or jump.</returns>
    private Vector3 AddGravityAndJump()
    {
        if (!flying)
        {
            grounded = Physics.CheckSphere(groundCheck.position, groundSphere, groundMask);

            if (grounded && velocity.y < 0)
            {
                velocity.y = 0;
            }

            velocity.y += gravity * Time.deltaTime;
        }
        else
        {
            grounded = false;
        }


        Jump();


        return velocity;
    }

    /// <summary>
    /// There are 3 main modes of jumping. The first mode is 
    /// when the player is not flying and is grounded. In this case the
    /// vertical velocity is set such that the height is equal to the jump height.
    /// 
    /// the second case is where the player is currently flying. In this case
    /// if the jump key is pressed then the player velocity will be set to the 
    /// positive flight velocity and when the jump key is released then the velocity
    /// will be set back to zero. This way the player will rise when the key is pressed
    /// and stay stationary when it is released.
    /// 
    /// The third method is when flight is possible and the doubleJump is initiated.
    /// Double jump is determined by the method Double jump. This will then toggle 
    /// the flying to whatever it wasn't. If the flying bool is now true then 
    /// the gravity code will not run and the y velocity will be reset to zero. If flying
    /// is false then gravity will be used. 
    /// 
    /// The last if statment will keep track of the last time the jump key was pressed. 
    /// 
    /// </summary>
    private void Jump()
    {
        bool jumpPressed = Input.GetButtonDown("Jump");
        bool doubleJump = DoubleJump(jumpPressed);
        firstJump = firstJump || jumpPressed;

        // case where the player is on the ground and not flying
        if (jumpPressed && grounded && !flying)
        {
            velocity.y = jumpVel;
        }

        // case where the player is currently flying
        if (flying)
        {
            if (jumpPressed && transform.position.y < 20)
            {
                velocity.y = flightVel;
            }

            if (Input.GetButtonUp("Jump") || (transform.position.y > 20 && !Input.GetButtonUp("Jump")))
            {
                velocity.y = 0;
            }
        }


        // case where flight is possible and the player doubleJumps
        if (flightPossible && doubleJump)
        {
            flying = !flying;
            if (flying)
            {
                velocity.y = 0;
            }
        }

        // logs the last time the jump key was pressed so that we can
        // keep track of double jumps
        if (jumpPressed)
        {
            lastJump = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }
    }

    /// <summary>
    /// This method will return true if the double jump was initiated. 
    /// This will happen if jump is pressed within a period that is less
    /// than the doublePressPeriod from the last time jump is pressed.
    /// 
    /// </summary>
    /// <param name="jumpPressed">whether or not the jump key has currenly been pressed</param>
    /// <returns>true if this key press is a double jump.</returns>
    private bool DoubleJump(bool jumpPressed)
    {
        if (!jumpPressed)
        {
            return false;
        }
        long currTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

        if (jumpPressed && currTime - lastJump < doublePressPeriod)
        {
            return true;
        }

        return false;
    }
    #endregion


}

