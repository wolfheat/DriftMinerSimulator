using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using Wolfheat.StartMenu;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] PlayerStats playerStats;
    [SerializeField] UIController uiController;
    [SerializeField] Transform tilt;
    [SerializeField] GameObject feet;
    //[SerializeField] private PickableItem genericPrefab;

    public bool InGravity { get { return rb.useGravity; } }
    // Player moves acording to velocity and acceleration
    private Vector2 screenCenter;
// Max speed
    float maxSpeed = 5f;
    Vector3 lastSafePoint = new Vector3();
    Vector3 boosterAcceleration = new Vector3();
    float boosterAccelerationSpeed = 5f;
    float walkingAccelerationSpeed = 1f;
    float dampening = 0.08f;
    float stopDampening = 6f;

    public float LookSensitivity { get; set; } = 0.15f;

    private const float StopingSpeedLimit = 0.1f; // go slower than this and you imidiately stop
    private const float DistanceLimit = 0.1f; // go slower than this and you imidiately stop
    private const float MaxDistanceLimit = 2.5f; // safe messure if moving to far away from throw point
    private const float RotationLowerLimit = 89;
    private const float RotationUpperLimit = 271;
    private const float WalkSpeed = 3.0f;
    private const float SprintSpeed = 7.0f;
    private const float WalkSpeedNeededToMakeStepSound = 0.3f;
    private const float JumpColliderRadius = 0.4f;
    private const float JumpForce = 7f;
    private const float JumpDelay = 0.3f;
        
    private float jumpTimer;
    private Coroutine throwCoroutine;
    [SerializeField] private LayerMask jumpables;

    public void OnEnable()
    {
        screenCenter = new Vector2();
        lastSafePoint = rb.transform.position;
        //jumpables = 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Interactables");
        Debug.Log("Jumpables set to: "+Convert.ToString(jumpables,2));
        Debug.Log("Mouse Sensitivity updated to: "+LookSensitivity);
        SavingUtility.LoadingComplete += LoadingComplete;
    }

    private void Start()
    {
        GameState.state = GameStates.Running;
    }

    private void LoadingComplete()
    {
        Debug.Log("Loading of settings complete update");
        LookSensitivity = SavingUtility.gameSettingsData.playerInputSettings.MouseSensitivity;
    }

    public bool HasFocus { get; set; }

    private void OnApplicationFocus(bool focus)
    {
        Debug.Log("Focus Set to "+focus);
        HasFocus = focus;
    }

    private void Update()
    {
        DetemineCursor();
        if(GameState.state == GameStates.Running)
            Look();
    }
    private void FixedUpdate()
    {
        Move();
    }

    private void DetemineCursor()
    {
        // Right button is not held or player is dead = regain normal cursor
        if (GameState.IsPaused || playerStats.IsDead)
        {
            if (Cursor.visible == true) return;

            //Debug.Log("Showing cursor");
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            //RegainCursorPosition();
            return;
        }
        else
        {
            if (!Cursor.visible) return;

            //Debug.Log("Hiding cursor");
            // Hide cursor if changing view
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

    }

    private void Move()
    {
        if (playerStats.IsDead) 
        {
            Stop();
            return;
        }

        Vector2 move = Inputs.Instance.Controls.Player.Move.ReadValue<Vector2>();
        

        // SIDEWAY MOVEMENT
        boosterAcceleration = move[0] * transform.right;
        // SPEED BOOSTER MOVEMENT
        boosterAcceleration += move[1] * transform.forward;

        float MoveSpeed = ShiftHeld() ? SprintSpeed : WalkSpeed;


        if (rb.useGravity)
        {

            //remove up and down
            boosterAcceleration = new Vector3(boosterAcceleration.x, 0, boosterAcceleration.z);
            
            // Movement in Gravity            
            rb.AddForce(boosterAcceleration.normalized * walkingAccelerationSpeed, ForceMode.VelocityChange);            

            // Limit movement speed
            Vector3 planeParts = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            if (planeParts.magnitude > MoveSpeed)
                planeParts = planeParts.normalized * MoveSpeed;

            // Add dampening to movement
            planeParts *= Mathf.Pow(dampening, Time.deltaTime);
            rb.velocity = new Vector3(planeParts.x, rb.velocity.y, planeParts.z);            

            // Jumping
            if(jumpTimer>0)
                jumpTimer-= Time.deltaTime;
            if(jumpTimer <= 0)
            {
                float upDown = Inputs.Instance.Controls.Player.UpDown.ReadValue<float>();
                if (upDown==1)
                {
                    
                    if (OnGround())
                    {
                        rb.AddForce(Vector3.up*JumpForce,ForceMode.Impulse);
                        jumpTimer = JumpDelay;
                    }
                }
            }
            /*
            if (boosterAcceleration.magnitude>0)
                rb.velocity = boosterAcceleration.normalized * minSpeed + rb.velocity.y * Vector3.up;
            else
                rb.velocity = rb.velocity.y * Vector3.up;
        }
        else if (planeParts.magnitude > maxSpeed)
            rb.velocity = planeParts.normalized * maxSpeed+rb.velocity.y*Vector3.up;
        */
            //DampenSpeedInDoors();

            // STEP SOUND
            if (planeParts.magnitude > WalkSpeedNeededToMakeStepSound && OnGround())
                SoundMaster.Instance?.PlayStepSound();

        }
        else
        {
            float upDown = Inputs.Instance.Controls.Player.UpDown.ReadValue<float>();
            boosterAcceleration += upDown * transform.up;

            // Movement in NON-Gravity
            rb.AddForce(boosterAcceleration.normalized * boosterAccelerationSpeed * Time.deltaTime, ForceMode.VelocityChange);
            // Limit velocity
            if (rb.velocity.magnitude > playerStats.MaxSpeed)
                rb.velocity = rb.velocity.normalized * playerStats.MaxSpeed;

            // CRUISE SPEED LIMITATION       
            if (rb.velocity.magnitude > playerStats.MaxSpeed/2f)
                LimitSpeedToCruiseSpeed(); 

            // PLAYER STOP IN PLACE
            if (Inputs.Instance.Controls.Player.LeftAlt.ReadValue<float>() != 0)
            StopInPlace();
        }

        // Show speed in HUD
        //uiController.SetSpeed(rb.velocity);

        // Limit angular rotations
        StopRotations();
    }

    private bool OnGround() => Physics.OverlapSphere(feet.transform.position, JumpColliderRadius, jumpables).Length > 0;

    private bool ShiftHeld() => Inputs.Instance.Controls.Player.Shift.ReadValue<float>() > 0;
        
    private bool StandingOnCollider()
    {
        bool hitCollider = Physics.Raycast(transform.position, Vector3.down, 1f);
        return hitCollider;
    }

    private void DampenSpeedInDoors()
    {
        Vector3 planeParts = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        planeParts *= Mathf.Pow(dampening, Time.deltaTime);
        rb.velocity = new Vector3(planeParts.x, rb.velocity.y, planeParts.z);
    }
    
    private void LimitSpeedToCruiseSpeed()
    {
        // Make different dampening in space and inside a spacestation
        if (boosterAcceleration.magnitude == 0)
        {
            rb.velocity *= Mathf.Pow(dampening, Time.deltaTime);
            return;
        }

        // Limit sideway velocity when using booster - Experimental
        /*
        Vector3 perpendicularSpeed = Vector3.Dot(rb.velocity.normalized, boosterAcceleration.normalized) * boosterAcceleration.normalized;
        rb.velocity -= perpendicularSpeed * driftDampening * Time.deltaTime;        
        */
    }

    private void StopRotations()
    {
        // Hinder player from falling down
        Vector3 startRot = rb.transform.rotation.eulerAngles;
        // Stop current rotation
        rb.angularVelocity = Vector3.zero;
        // Reset player to upright position
        rb.transform.rotation = Quaternion.Euler(0, rb.transform.rotation.eulerAngles.y, 0);
    }

    private void StopInPlace()
    {
        if (rb.velocity.magnitude <= StopingSpeedLimit)
        {
            rb.velocity = Vector3.zero;
            return;
        }
        // Slow Down player
        float newVel = rb.velocity.magnitude - stopDampening * Time.deltaTime;
        rb.velocity = rb.velocity.normalized * newVel;
        
    }
    
    private void Stop()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int X, int Y);

    public void Look()
    {
        // Is this sensitiv to when cursor wont show up and paused = yes
        if (Cursor.visible == true) return;
        
        // When holding right button rotate player by mouse movement
        Vector2 mouseMove = Inputs.Instance.Controls.Player.Look.ReadValue<Vector2>();
        if (mouseMove.magnitude != 0)
        {
            // ISSUE Lower part of screen goes from 270-360 upper part 0-90. Issue to limit looking up and down past boundaries
            // 0.675 -> -0.675
            // 270 -> 90

            // Looking to the sides
            rb.transform.Rotate(0, mouseMove[0] * LookSensitivity, 0, Space.Self);

            // Looking up and down
            float oldAngle = tilt.localRotation.eulerAngles.x;
            float rotationAngle = (-mouseMove[1] * LookSensitivity);
            
            float resultAngle = oldAngle + rotationAngle;
            if (rotationAngle > 0 && oldAngle <= RotationLowerLimit + 1 && oldAngle >= RotationLowerLimit - 20f && resultAngle >= RotationLowerLimit)
            {
                //Debug.Log("Changing valid rotationangle");
                rotationAngle = RotationLowerLimit - oldAngle;
            }
            else if (rotationAngle < 0 && oldAngle >= RotationUpperLimit - 1 && oldAngle <= RotationUpperLimit + 20f && resultAngle <= RotationUpperLimit)
            {
                //Debug.Log("Changing to max rotationangle");

                rotationAngle = RotationUpperLimit - oldAngle;
            }
            tilt.transform.Rotate(rotationAngle, 0, 0, Space.Self);
            //uiController.SetTilt(tilt.transform.localRotation.eulerAngles.x);
            //uiController.SetPlayerTilt(rb.transform.localRotation.eulerAngles);
            // At 334 degrees

        }
    }


    public void RegainCursorPosition()
    {
        Mouse.current.WarpCursorPosition(screenCenter);
    }

    public void SetToSafePoint()
    {
        Debug.Log("Setting player to safepoint: " + lastSafePoint);

        rb.transform.position = lastSafePoint;
    }

}
