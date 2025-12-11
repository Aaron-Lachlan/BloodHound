using UnityEngine;

public class Teleporter : MonoBehaviour
{

    public GameObject Tgt;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    private void OnTriggerEnter(Collider player)
    {
        player.transform.position = Tgt.transform.position;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Rotate(0, +1, 0);
    }
    
}
