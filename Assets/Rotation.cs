using _CharacterController;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class Rotation : MonoBehaviour
{
    public GameObject GameCamera;

    private float turnSmoothVelocity;
    public float turnSpeed = 0.1f;
    public Rigidbody RB;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (RB.linearVelocity.magnitude >= 0.01f)
        {

            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, GameCamera.transform.eulerAngles.y, ref turnSmoothVelocity, turnSpeed);
            Quaternion moveAngle = Quaternion.Euler(0, smoothAngle, 0);



            transform.localRotation = moveAngle;


        }


    }
}
