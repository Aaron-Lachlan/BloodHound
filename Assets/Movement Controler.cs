using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cinemachine;

public class MovementControler : MonoBehaviour
{
    public enum MovementState
    {
        Idle,
        Stationery,
        Walking,
        Running,
        Jumping,
        Falling,
        Crouching,
        Sliding,

    }
    public MovementState currentState;
    public TextMeshProUGUI stateText;

    [Header("Speed Stuff")]
    public float accelerationSpeed = 12f;
    public float decelerationSpeed = 5f;
    public float topSpeed = 12f;
    public float maxSpeed = 12f;

    [Header("Jump Stuff")]
    public float jumpForce = 4;
    public float timeHeld = 5;
    private float timeHeldTimer;


    [Header("Input")]
    float horizontalInput;
    float verticalInput;

    private Rigidbody RB;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;


    private Vector3 moveDirection;
    private Transform orientation;

    [Header("Camra Rotation")]
    public GameObject cameraa;
    public CinemachineFreeLook cameraControl;
    private float turnSmoothVelocity;
    public float turnSpeed = 0.1f;
    private float idleTimer;

    [Header("Test")]
    public float groundDrag;
    public float airDrag;
    public int moveMulti;

    public float gravity;

    void Start()
    {
        if (RB == null)
        {
            RB = GetComponent<Rigidbody>();
        } 
        if (orientation == null)
        {
            orientation = GetComponent<Transform>();
        }
        idleTimer = cameraControl.m_YAxisRecentering.m_WaitTime;
    }


    void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        
        InputManager();
        UpdateState();


    }

    private void FixedUpdate()
    {
        //gravity
        RB.AddForce(0, gravity, 0);

        //move add up into vector 3
        Vector3 cameraForward = cameraa.transform.forward;
        cameraForward.y = 0f;
        moveDirection = cameraForward.normalized * verticalInput + cameraa.transform.right * horizontalInput;

        //Camera rotation
        if (moveDirection.magnitude >= 0.1f)
        {

            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, cameraa.transform.eulerAngles.y, ref turnSmoothVelocity, turnSpeed);
            Quaternion moveAngle = Quaternion.Euler(0, smoothAngle, 0);
            RB.MoveRotation(moveAngle);

        }



        //character movement
        if (grounded)
        {
            RB.AddForce(moveDirection.normalized * moveMulti);
        }
        else
        {
            RB.AddForce(moveDirection.normalized * 5);
            
        }
    }

    void InputManager()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        stateText.text = currentState.ToString();



    }


    void UpdateState()
    {

        switch (currentState)
        {
            case MovementState.Idle:
                if (moveDirection.magnitude >= 0.1f)
                {
                    currentState = MovementState.Stationery;
                }


                    break;


            case MovementState.Stationery:


                #region IDLE CHECK
                if (moveDirection.magnitude <= 0.1f)
                {

                    idleTimer -= Time.deltaTime;

                    if (idleTimer <= 0)
                    {
                        idleTimer = cameraControl.m_YAxisRecentering.m_WaitTime;
                        currentState = MovementState.Idle;
                    }
                }
                if (idleTimer != cameraControl.m_YAxisRecentering.m_WaitTime)
                {
                    if(moveDirection.magnitude >= 0.1f)
                    {
                        idleTimer = cameraControl.m_YAxisRecentering.m_WaitTime;
                    }
                }
                #endregion


                if (!grounded)
                {
                    RB.drag = airDrag;
                    currentState = MovementState.Falling;
                }

                if (Input.GetButtonDown("Jump"))
                {
                    RB.drag = airDrag;
                    timeHeldTimer = timeHeld;
                    RB.velocity = new Vector3(RB.velocity.x, RB.velocity.y + jumpForce, RB.velocity.z);
                    currentState = MovementState.Jumping;
                }


                break;


            case MovementState.Walking:
                // Code for the Walking state
                break;


            case MovementState.Running:
                // Code for the Running state
                break;


            case MovementState.Jumping:
                if (Input.GetButton("Jump") && currentState == MovementState.Jumping)
                {
                    if (timeHeldTimer > 0)
                    {
                        RB.velocity = new Vector3(RB.velocity.x, RB.velocity.y + (jumpForce * Time.deltaTime), RB.velocity.z);
                        timeHeldTimer -= Time.deltaTime;
                    }
                    else
                    {
                        currentState = MovementState.Falling;

                    }
                    if (grounded && timeHeldTimer < (timeHeld / 1.1f))
                    {
                        Debug.Log("Trigger");
                        currentState = MovementState.Falling;
                    }
                }
                if (Input.GetButtonUp("Jump"))
                {
                    currentState = MovementState.Falling;
                }


                break;


            case MovementState.Falling:

                if (grounded)
                {
                    RB.drag = groundDrag;
                    currentState = MovementState.Stationery;
                }
                break;

            case MovementState.Crouching:
                // Code for the Jumping state
                break;

            case MovementState.Sliding:
                // Code for the Jumping state
                break;

            // Add more cases for other states


            default:
                
                
                break;
        }



    }

}
