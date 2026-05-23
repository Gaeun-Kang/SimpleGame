using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    
    private Rigidbody rb;
    [SerializeField] private float movespeed = 5.0f;
    private float X_Axis;
    private float Z_Axis;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        X_Axis = Input.GetAxis("Horizontal");
        Z_Axis = Input.GetAxis("Vertical");
    
    Vector3 velocity = new Vector3(X_Axis, 0, Z_Axis);
    velocity *= movespeed;
    rb.velocity = velocity;
    
    }
}
