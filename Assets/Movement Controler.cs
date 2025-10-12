using System;
    using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace _CharacterController
{


    public class MovementControler : MonoBehaviour
    {
        public enum MovementState
        {
            Idle,
            Stationery,
            Moving,
            Jumping,
            Falling,
            Crouching,
            Sliding,
            SlamDunk,

        }
        public MovementState currentState;
        public TextMeshProUGUI stateText;
        private Rigidbody RB;

        [Header("Speed Stuff")]
        public float accelerationSpeed = 12f;
        public float decelerationSpeed = 5f;
        public float topSpeed = 12f;
        public float maxSpeed = 12f;
        [SerializeField] private float currentSpeed;

        [Header("Jump Stuff")]
        public float jumpForce = 4;
        public float timeHeld = 1;

        [SerializeField] private float gravity;
        public float NormalGravity;
        public float FallingGravity;
        private float timeHeldTimer;

        [Header("Crouch Stuff")]
        [Tooltip("Devide base movespeed by x when Crouched")]
        public float CrouchSpeed = 2;


        [Header("Input")]
        public InputActionAsset InputActions;
        bool allowInput = true;

        private InputAction MoveAction;
        private InputAction JumpAction;
        private InputAction CrouchAction;
        

        private float verticalInput;
        private float horizontalInput;

        [Header("Ground Check")]
        public LayerMask whatIsGround;
        public bool grounded;
        private float castLength = 0.9f;


        private Vector3 moveDirection;
        private Transform orientation;

        [Header("Camra Rotation")]
        public GameObject GameCamera;
        public CinemachineOrbitalFollow CinMachCamera;
        private float turnSmoothVelocity;
        public float turnSpeed = 0.1f;
        

        [Header("Idle Stuff")]
        public float TimeTillIdle = 6f;
        private float idleTimer;


        [Header("Slam Dunk")]
        public float DunkSpeed = 75f;

        [Header("Test")]
        public float groundDrag;
        public float airDrag;
        public int moveMulti;

       

        [Header("Normal Spap")]
        public AnimationCurve aniCurve;
        public float time;

        private RaycastHit hit;



        private void Start()
        {
            if (RB == null)
            {
                RB = GetComponent<Rigidbody>();
            }
            if (orientation == null)
            {
                orientation = GetComponent<Transform>();
            }


            //sets idle times
            CinMachCamera.VerticalAxis.Recentering.Wait = TimeTillIdle;
            CinMachCamera.HorizontalAxis.Recentering.Wait = TimeTillIdle;
            idleTimer = TimeTillIdle;
        }
        private void OnEnable()
        {
            InputActions.FindActionMap("Player").Enable();


        }
        private void OnDisable()
        {
            InputActions.FindActionMap("Player").Disable();
        }
        private void Awake()
        {
            MoveAction = InputSystem.actions.FindAction("Move");
            JumpAction = InputSystem.actions.FindAction("Jump");
            CrouchAction = InputSystem.actions.FindAction("Crouch");

    
        }





        void Update()
        {
            grounded = Physics.SphereCast(transform.position, 0.5f, Vector3.down, out hit, castLength, whatIsGround);

            InputManager();
            UpdateState();
            //OnDrawGizmos();
        }

        private void FixedUpdate()
        {
            //gravity
            RB.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

            //build vector 3 for move/direction
            Vector3 cameraForward = GameCamera.transform.forward;
            cameraForward.y = 0f;
            moveDirection = cameraForward.normalized * verticalInput + GameCamera.transform.right * horizontalInput;

            //Character rotation
            if(moveDirection.magnitude >= 0.1f)
            {
                
                float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, GameCamera.transform.eulerAngles.y, ref turnSmoothVelocity, turnSpeed);
                Quaternion moveAngle = Quaternion.Euler(transform.eulerAngles.x, smoothAngle, transform.eulerAngles.z);

                transform.localRotation = moveAngle;

                
            }



            //character movement
            if (grounded)
            {
                //transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                Quaternion RotationRef = Quaternion.Euler(0, 0, 0);

                RotationRef = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(Vector3.up, hit.normal), aniCurve.Evaluate(time));
                transform.rotation = Quaternion.Euler(RotationRef.eulerAngles.x, transform.eulerAngles.y, RotationRef.eulerAngles.z);





                
                if (currentState != MovementState.Sliding)
                {
                    RB.AddForce(moveDirection.normalized * 100);
                }
                else
                {
                    RB.AddForce((moveDirection.normalized * 100) /CrouchSpeed);
                    
                }

            }
            else
            {
                RB.AddForce(moveDirection.normalized * 5);
                
            }
        }

        /*
        
        void OnDrawGizmos()
        {
            Vector3 test = new Vector3(transform.position.x, transform.position.y - castLength, transform.position.z);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(test, 0.5f);
        }
        */


        void InputManager()
        {
            horizontalInput = MoveAction.ReadValue<Vector2>().x;
            verticalInput = MoveAction.ReadValue<Vector2>().y;

            if (Input.GetKeyDown("escape"))
            {
                SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);

            }

            stateText.text = currentState.ToString();

        }

        private void Jump(float a)
        {
            RB.linearDamping = airDrag;
            timeHeldTimer = timeHeld;
            RB.AddForce(transform.up * (jumpForce + a), ForceMode.Impulse);
            currentState = MovementState.Jumping;

        }

        #region SLAM DUNK
        private IEnumerator SlamDunk()
        {
            float peakHeight = transform.position.y;
            allowInput = false;

            //downward force 
            RB.linearVelocity = new Vector3(0, -DunkSpeed, 0);

            yield return new WaitUntil(() => grounded);
            float ground = transform.position.y;
            float distance = peakHeight - ground;


            StartCoroutine(ParallelTimer());
            while (!allowInput)
            {

                if (JumpAction.WasPressedThisFrame())
                {
                    Jump(distance);
                    allowInput = true;
                    break;

                }

                yield return null;
            }
        }

        private IEnumerator ParallelTimer()
        {
            float timerDuration = 1f;
            float timerElapsed = 0f;

            while (timerElapsed < timerDuration)
            {
                if (allowInput)
                {
                    yield break;
                }

                timerElapsed += Time.deltaTime;
                yield return null;
            }

            allowInput = true;

        }



        #endregion



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
                            idleTimer = TimeTillIdle;
                            currentState = MovementState.Idle;
                        }
                    }
                    if (idleTimer != TimeTillIdle && moveDirection.magnitude >= 0.1f)
                    {
                            idleTimer = TimeTillIdle;
                    }
                    #endregion


                    if (!grounded)
                    {
                        RB.linearDamping = airDrag;
                        currentState = MovementState.Falling;
                    }

                    if (JumpAction.WasPressedThisFrame())
                    {
                        Jump(0);
                    }

                    if (CrouchAction.WasPressedThisFrame())
                    {
                        currentState = MovementState.Crouching;

                    }
                    

                    break;


                case MovementState.Moving:
                    // Code for the moving state
                    break;


                    
                case MovementState.Jumping:
                    if (JumpAction.IsPressed())
                    {
                        if (timeHeldTimer >= 0)
                        {
                            RB.linearVelocity = new Vector3(RB.linearVelocity.x, RB.linearVelocity.y + (jumpForce * Time.deltaTime), RB.linearVelocity.z);
                            timeHeldTimer -= Time.deltaTime;
                            
                        }
                        else
                        {
                            currentState = MovementState.Falling;

                        }
                    }
                    if (JumpAction.WasReleasedThisFrame())
                    {
                        
                        currentState = MovementState.Falling;
                    }


                    break;


                case MovementState.Falling:

                    gravity = FallingGravity;

                    if (grounded)
                    {
                        RB.linearDamping = groundDrag;
                        gravity = NormalGravity;
                        currentState = MovementState.Stationery;
                    }

                    if (CrouchAction.WasPressedThisFrame())
                    {
                        gravity = NormalGravity;
                        currentState = MovementState.Crouching;

                    }

                    break;

                case MovementState.Crouching:

                    if (!grounded)
                    {
                        
                        StartCoroutine(SlamDunk());
                 
                        currentState = MovementState.Falling;
                    }
                    else
                    {
                        currentState = MovementState.Sliding;

                    }


                    break;



                case MovementState.Sliding:

                    RB.linearDamping = airDrag;

                    if (CrouchAction.WasReleasedThisFrame())
                    {
                        RB.linearDamping = groundDrag;
                        currentState = MovementState.Stationery;
                    }

                    break;




                case MovementState.SlamDunk:

                   //to do move slamdunk to here


                    break;


                default:


                    break;
            }

            //to do list

            //slam dunk
                // no input timer broken 
                //move into state
            //crouch
                //add movement on slopes normal direction
                //add max speed for crouch walking that trans into sliding
                //maybe add small hop when press jump
            //falling
                //when falling down a steep slop, it isnt smooth like sliding down it

            //character ground normal roation and look direction conflicting
            //Stationery and moving state are currently one is the same/ need to split
            //I belive that character turning and camera turning is having a little feedback loop
            //add slow camera spin when idle
        }

    }

}
