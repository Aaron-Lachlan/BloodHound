using System;
    using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using TMPro;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace _CharacterController
{


    public class MovementControler : MonoBehaviour
    {

        public enum MovementState
        {
            IdleStage2,
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

        [Header("Speed")]
        [SerializeField] private float CurrentMoveSpeed;

        public float MoveSpeed = 4500;
        public float AirSpeed = 750;
        
        

        //public float accelerationSpeed = 12f;
        //public float decelerationSpeed = 5f;
        //public float topSpeed = 12f;
        //public float maxSpeed = 12f;
        //[SerializeField] private float currentSpeed;

        [Header("Jump")]
        [SerializeField] private bool CanJump;
        [SerializeField] private bool IsJumping;
        public float jumpForce = 1100;
        public float VariableJumpHeightForce = 7.5f;
        public float timeHeld = 0.3f;

        private float timeHeldTimer;

        [Header("Gravity")]
        [SerializeField] private float gravity;
        public float JumpingGravity = 9.8f;
        public float FallingGravity = 40f;
        

        [Header("Drags")]
        public float groundDrag = 3.5f;
        public float airDrag = 0.01f;


        [Header("Crouch Stuff")]
        public float CrouchSpeed = 200;
        public float SlideSpeed = 200;
        public float SlideDrag = 0.5f;
        public float CrouchToSlideSpeedThreshold = 6;
        
        [SerializeField] private bool CanCrouch;
        [SerializeField] private bool IsCrouching;

        [Header("Visual to replace")]
        public GameObject Parent;
        public GameObject SlidePartileFab;
        private GameObject SlidePartile;

        [Header("Slope")]

        public float WalkNormalAngleInfluenceThreshold = 25;
        public float CrouchAngleInfluenceThreshold = 10;
        public float CrouchOnAngleSpeed = 100;
        public float WalkOnAngleSpeed = 25;

        private float AngleSpeed;
        private float AngleInfluenceThreshold;



        [Header("Input")]
        private Vector2 moveAmount;
        private Vector3 moveDirection;

        [Header("Camra Rotation")]
        public GameObject GameCamera;
        public CinemachineOrbitalFollow CinMachCamera;


        [Header("Idle Stuff")]
        public float TimeTillIdle = 10f;
        public float TimeRecenterTakes = 2f;
        public float IdleCameraSpinSpeed = 2f;
        private float idleTimer;


        [Header("Slam Dunk")]
        //public float DunkSpeed = 75f;
        //bool allowInput = true;

        [Header("Normal Snap and air normal reset")]
        public AnimationCurve AniCurve;
        private float elapsedTime;
        private quaternion NormalAngle;



        [Header("Raycast Ground")]
        private RaycastHit hit;
        private RaycastHit lastHit;

        private float castLength = 0.9f;
        public float RaySize = 0.5f;

        public LayerMask whatIsGround;
        public bool grounded;



        private void Start()
        {
            if (RB == null)
            {
                RB = GetComponent<Rigidbody>();
            }

            

            //Turn back on when build
            //Cursor.lockState = CursorLockMode.Locked;
            //Cursor.visible = false;

            RB.freezeRotation = true;

            //sets idle times
            CinMachCamera.VerticalAxis.Recentering.Wait = TimeTillIdle;
            CinMachCamera.VerticalAxis.Recentering.Time = TimeRecenterTakes;
            CinMachCamera.HorizontalAxis.Recentering.Wait = TimeTillIdle;
            CinMachCamera.HorizontalAxis.Recentering.Time = TimeRecenterTakes;
            idleTimer = TimeTillIdle;
        }

        void Update()
        {

            MapReloader();

            VisualToReplace();
        }


        private void FixedUpdate()
        {

            //ground check
            grounded = Physics.SphereCast(transform.position, RaySize, -transform.up, out hit, castLength, whatIsGround);



            //snapping to the ground normal and normal reset when in air
            NormalSnap();

            //states have their own Damp/grav variables, this function handles that
            DampingGravityChange();

            UpdateState();

            SlopeSlide();

            //gravity
            RB.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

            CharacterMovement();

        }
        void DampingGravityChange()
        {
            if (grounded)
            {
                gravity = JumpingGravity;

                if (currentState != MovementState.Sliding)
                {

                    RB.linearDamping = groundDrag;
                    

                }
                else
                {

                    RB.linearDamping = SlideDrag;

                }
            }
            else
            {
                if(!IsJumping )
                {
                    gravity = FallingGravity;
                }
                RB.linearDamping = airDrag;
                

                
            }

        }
        void SlopeSlide()
        {
            if (grounded)
            {
                float slopeDot = Vector3.Dot(hit.normal, Vector3.up);
                slopeDot = Mathf.Acos(slopeDot) * Mathf.Rad2Deg;

                if (IsCrouching)
                {
                    AngleInfluenceThreshold = CrouchAngleInfluenceThreshold;

                }
                else
                {

                    AngleInfluenceThreshold = WalkNormalAngleInfluenceThreshold;
                }



                if (slopeDot > AngleInfluenceThreshold)
                {
                    
                    Vector3 slideDir;
                    slideDir = Vector3.ProjectOnPlane(Vector3.down, hit.normal);

                    if(IsCrouching)
                    {
                        AngleSpeed = CrouchOnAngleSpeed;

                    }
                    else
                    {

                        AngleSpeed = WalkOnAngleSpeed;
                    }
                        RB.AddForce(slideDir * AngleSpeed, ForceMode.Acceleration);
                }


            }



        }
        void VisualToReplace()
        {

            //just some dumb visuals that will be moved somewhere else later on

            if (IsCrouching)
            {
                transform.localScale = new Vector3(1, 0.5f, 1);

            }
            else
            {
                transform.localScale = Vector3.one;
            }

            if (currentState == MovementState.Sliding & grounded)
            {


                if (SlidePartile == null)
                {



                    Vector3 newVel = new Vector3(0, 0.25f, -1);

                    Vector3 test = new Vector3(0f, -0.2f, -0.7f);
                    test = transform.position + test;
                    SlidePartile = Instantiate(SlidePartileFab, test, Quaternion.LookRotation(newVel), Parent.transform);

                }

            }
            else if (SlidePartile != null)
            {
                if (!grounded || currentState != MovementState.Sliding)
                {
                    Destroy(SlidePartile);
                }

            }
 
        }
        void NormalSnap()
        {


            if (hit.normal != lastHit.normal && hit.normal != Vector3.zero)
            {

                elapsedTime = 0f;
                NormalAngle = Quaternion.FromToRotation(Vector3.up, hit.normal);


            }
            else if (!IsJumping & !grounded)
            {
                elapsedTime = 0f;
                NormalAngle = Quaternion.identity;

            }



            elapsedTime += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / 1);
            float curveValue = AniCurve.Evaluate(t);


            RB.MoveRotation(Quaternion.Lerp(transform.rotation, NormalAngle, curveValue));

            lastHit = hit;



            //make it ignore all 50+ degree angles
        }

        /*
        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(hit.point, 0.2f);


            if (!Application.isPlaying) return;

            var position = transform.position;
            var velocity = RB.linearVelocity;

            if (velocity.magnitude < 0.1f) return;

            Handles.color = Color.red;
            Handles.ArrowHandleCap(0, position, Quaternion.LookRotation(velocity), RB.linearVelocity.magnitude / 10, EventType.Repaint);

            if (moveDirection != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(moveDirection, transform.up);
                Handles.color = Color.blue;
                Handles.ArrowHandleCap(0, position, lookRotation, 1, EventType.Repaint);
            }
        }
        */
        void IdleTimer(int Stage)
        {
            if (moveDirection.magnitude <= 0.1f)
            {

                idleTimer -= Time.fixedDeltaTime;

                if (idleTimer <= 0)
                {
                    idleTimer = TimeTillIdle;
                    if (Stage == 1)
                    {
                        currentState = MovementState.Idle;

                    }
                    else
                    {
                        currentState = MovementState.IdleStage2;

                    }

                }
            }
            if (idleTimer != TimeTillIdle && moveDirection.magnitude >= 0.1f)
            {
                idleTimer = TimeTillIdle;
            }

        }

        void MapReloader()
        {
            

            if (Input.GetKeyDown("escape"))
            {
                SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);

            }

            stateText.text = currentState.ToString();

        }

        /*
        
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

                timerElapsed += Time.fixedDeltaTime;
                yield return null;
            }

            allowInput = true;

        }
        */



        //#endregion

        #region Jump Movement

        public void OnJump(InputAction.CallbackContext context)
        {
            
            if (CanJump & context.started)
            {
                
                

                
                timeHeldTimer = timeHeld;
                RB.AddForce(transform.up * jumpForce, ForceMode.Impulse);

                
                IsJumping = true;
                currentState = MovementState.Jumping;


            }
            if (context.canceled)
            {
                IsJumping = false;

            }
        }
        void VariableJumpHeight()
        {
            


            if (IsJumping)
            {
                if (timeHeldTimer >= 0)
                {
                    RB.AddForce(transform.up * VariableJumpHeightForce, ForceMode.Acceleration);
                    timeHeldTimer -= Time.fixedDeltaTime;
                }
                else
                {
                    IsJumping = false;
                    currentState = MovementState.Falling;

                }
            }
            else
            {
                IsJumping = false;
                currentState = MovementState.Falling;

            }

        }

        #endregion
        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (CanCrouch)
            {
                if (context.performed)
                {
                    IsCrouching = true;

                    currentState = MovementState.Crouching;
                    
                    
                }
                else if (context.canceled)
                {
                    IsCrouching = false;
                    
                    

                    //when it has these 1 frame trans from crouching to Stationery for one frame to moving
                    //may become an issue for animation trans?
                    currentState = MovementState.Stationery;
                    
                }

            }
            
        }

        #region WASD Movement
        public void OnMove(InputAction.CallbackContext context)
        {
            //movement Vectore 2 Updating
            if (context.performed)
            {
                moveAmount = context.ReadValue<Vector2>();
            }
            else if (context.canceled)
            {
                moveAmount = Vector2.zero;
            }
        }
        void CharacterMovement()
        {

            //build vector 3 for move/direction
            Vector3 cameraForward = GameCamera.transform.forward;
            Vector3 cameraRight = GameCamera.transform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;

            cameraForward = cameraForward.normalized;
            cameraRight = cameraRight.normalized;

            moveDirection = cameraForward * moveAmount.y + cameraRight * moveAmount.x;

            //character movement
            if (grounded)
            {

                

                if (!IsCrouching)
                {
                    CurrentMoveSpeed = MoveSpeed;
                }
                else
                {
                    if(currentState == MovementState.Crouching)
                    {
                        CurrentMoveSpeed = CrouchSpeed;
                    }
                    else
                    {
                        CurrentMoveSpeed = SlideSpeed;

                    }
                        
                }

            }
            else
            {
                CurrentMoveSpeed = AirSpeed;

            }

            RB.AddRelativeForce(moveDirection.normalized * CurrentMoveSpeed);
        }
        #endregion


        void UpdateState()
        {
            //try to keep all code out of UpdateState() and have Updated relyant functions called from here
            //also keep all 'Can' bools in here unless its for used only function like the 'IsJumping'

            switch (currentState)
            {
                case MovementState.IdleStage2:


                    CinMachCamera.HorizontalAxis.Value = CinMachCamera.HorizontalAxis.Value + (IdleCameraSpinSpeed * Time.fixedDeltaTime);

                    if (RB.linearVelocity.magnitude >= 0.01f)
                    {
                        currentState = MovementState.Moving;
                    }

                    break;

                case MovementState.Idle:


                    IdleTimer(2);

                    if (RB.linearVelocity.magnitude >= 0.01f)
                    {
                        currentState = MovementState.Moving;
                    }

                    break;


                case MovementState.Stationery:

                    CanJump = true;
                    CanCrouch = true;

                    IdleTimer(1);
                    

                    if (RB.linearVelocity.magnitude >= 0.01f)
                    {
                        currentState = MovementState.Moving;
                    }
                    if (!grounded)
                    {

                        currentState = MovementState.Falling;
                    }



                    break;


                case MovementState.Moving:

                    CanJump = true;
                    CanCrouch = true;

                    if (RB.linearVelocity.magnitude <= 0.01f)
                    {
                        currentState = MovementState.Stationery;
                    }
                    if (!grounded)
                    {
                        
                        currentState = MovementState.Falling;
                    }
                    

                    break;



                case MovementState.Jumping:

                    CanCrouch = false;
                    CanJump = false;

                    VariableJumpHeight();


                    break;


                case MovementState.Falling:

                    CanJump = false;
                    CanCrouch = true;

                    

                    if (grounded)
                    {
                        if (IsCrouching)
                        {
                            currentState = MovementState.Crouching;

                        }
                        else if (RB.linearVelocity.magnitude <= 0.01f)
                        {
                            currentState = MovementState.Stationery;

                        }
                        else
                        {
                            currentState = MovementState.Moving;  
                        }
                            

                    }

                    break;

                case MovementState.Crouching:

                    CanJump = true;


                    if (RB.linearVelocity.magnitude >= CrouchToSlideSpeedThreshold)
                    {
                        currentState = MovementState.Sliding;

                    }
                    break;



                case MovementState.Sliding:


                    


                    if (RB.linearVelocity.magnitude <= CrouchToSlideSpeedThreshold)
                    {
                        currentState = MovementState.Crouching;

                    }

                    break;




                case MovementState.SlamDunk:

                    //to do move slamdunk to here


                    break;


                default:


                    break;
            }


            //to do list

            //movement 
            //counter movement (dont think its needed but may have use case)
            //deStick from wall when wall is 90 degress and under a speed limit

            //slam dunk
            // no input timer broken 
            //move into state





            



            /*
            added
                Normal stuff
                    normal allining to the ground
                    normal allinging when in air to world space
                    rotation now works independent from normal alling
                    jump is normal allined
                movement
                    moved to new input system with event callbacks
                    RB movement is now applyed local and afected by normal allining
                    added movement state
                    added crouch state
                    movement inputs are kept track off, so you can hold crouch, jump to jump/fall state, and when landing returning to crouch state
                debug Gismoz!!!
                idle
                    Stage 2 with spin
                new input system
                    rebuild of switch and how input events change that
                Visuals
                    crouch
                    slide sparks
                updated to CinMachine 3.1
                switch to URP render pipeline
                fixed camera turning having a little feedback loop
                just made camera a little better
            Bugs: alot

            */
        }

    }
}


