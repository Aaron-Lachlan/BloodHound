using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IEnumeratorss : MonoBehaviour
{
    public bool boolien;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Name());
        //OR
        StartCoroutine("Name");

        //creates and starts instance of Name coroutine
        //corotines are just a fancy update method that gives 
        //access to update sensitive functions like Waituntill etc.
    }


    public IEnumerator Name()
    {



        yield return new WaitForSeconds(2);
        //code will wait for 2 seconds before continuing 
        //there are a bunch of waitfor ect.



        while (boolien == true)
        {

            yield return null;
            //return null waits for next frame

        }


        yield return null;
        //IEumeratons gotta have 1 yeid return null in them, it marks the end of the path
    }

}
