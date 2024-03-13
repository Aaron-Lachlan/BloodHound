using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controls : MonoBehaviour
{
   
    // These variables (visible in the inspector) are for you to set up to match the right feel
    public float accelerationSpeed = 12f;
    public float decelerationSpeed = 5f;
    public float topSpeed = 12f;
    public float maxSpeed = 12f;


    public float minJumpHeight = 4;
    public float maxJumpHeight = 4;




    //camra ref
    public Transform cam;
    // Customisable gravity
    public float gravity = -35;
    // How high the player can jump
    public float jumpHeight = 4.5f;
    //player rotation speed to camera
    public float turnSpeed = 1f;

   
    private CharacterController controller;
    private Vector3 velocity;
    private float turnSmoothVelocity;
    private bool allowInput = true;

    private bool isCrouched;
    
    




    private void Start()
    {
        // If the variable "controller" is empty...
        if (controller == null)
        {
            // ...then this searches the components on the gameobject and gets a reference to the CharacterController class
            controller = GetComponent<CharacterController>();

        }
    }



    private void Update()
    {


        #region Inputs
        if (allowInput)
        {


            // Get the Left/Right and Forward/Back values of the input being used (WASD, Joystick etc.)
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");


            // Let the player jump if they are on the ground and they press the jump button
            if (Input.GetButtonDown("Jump") && controller.isGrounded)
            {
                Jump(0);
            }





            // This takes the Left/Right and Forward/Back values to build a vector
            Vector3 move = new Vector3(x, 0, z).normalized;


            //character rotaion crap
            if (move.magnitude >= 0.1f)
            {
                //Rotation
                float angle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, angle, ref turnSmoothVelocity, turnSpeed);
                transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);

                Vector3 moveDirection = Quaternion.Euler(0, angle, 0) * Vector3.forward;

                // Finally, it applies that vector it just made to the character
                controller.Move(moveDirection * maxSpeed * Time.deltaTime);
            }


            if (Input.GetButtonDown("Crouch") && controller.isGrounded)
            {
                Crouch();
               
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                transform.localScale = new Vector3(1, 1.5f, 1);

            }



            //slam dunk BITCHHHhhhhh
            if (Input.GetButtonDown("Crouch") && !controller.isGrounded)
            {
                StartCoroutine(SlamDunk());
            }
            




        }
        #endregion



        //applying gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);




        //Slip();


    }





    private void Jump(float extra)
    {
        velocity.y = Mathf.Sqrt((jumpHeight + extra) * -2 * gravity);

    }


    private void Crouch()
    {
        


            transform.localScale = new Vector3(1, 1, 1);



        


    }






    #region SLAM DUNK
    private IEnumerator SlamDunk()
    {
        float peakHeight = transform.position.y;
        allowInput = false;

        //downward force 
        velocity.y += -50;
        
        yield return new WaitUntil(() => controller.isGrounded);
        float ground = transform.position.y;
        float distance = peakHeight - ground;
        

        StartCoroutine(ParallelTimer());
        while (!allowInput)
        {
            
            if (Input.GetButtonDown("Jump"))
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


}