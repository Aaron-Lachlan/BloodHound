using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Mathematics;

public class Spedometer : MonoBehaviour
{
   
    public TextMeshProUGUI texts;
    private Rigidbody RB;


    // Start is called before the first frame
    void Start()
    {
        if (RB == null)
        {
            RB = GetComponent<Rigidbody>();
        }
    }
    private void FixedUpdate()
    {
        float a;
        a = Mathf.Floor(RB.linearVelocity.magnitude);

        texts.text = a.ToString();
    }


}