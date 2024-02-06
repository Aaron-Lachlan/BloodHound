using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Spedometer : MonoBehaviour
{
    public float speed;
    public TextMeshProUGUI texts;
    

    // Start is called before the first frame
    void Start()
    {
        StartCoroutine(CalcSpeed());
    }

    IEnumerator CalcSpeed()
    {
        bool isPlaying = true;

        while (isPlaying)
        {
            Vector3 prevPos = transform.position;

            yield return new WaitForFixedUpdate();

            speed = Mathf.RoundToInt(Vector3.Distance(transform.position, prevPos) / Time.fixedDeltaTime);
            
            texts.text = speed.ToString();
            
        }   
    }
}