using UnityEngine;

public class InputManager : MonoBehaviour
{


    public Vector3 LookDirection { get; set; } 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Look()
    {
        LookDirection = this.transform.forward;
    }
}
