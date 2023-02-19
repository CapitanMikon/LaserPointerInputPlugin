using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField] private float force;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        rb.useGravity = true;
        rb.AddExplosionForce(force, transform.position, 1f, 0f, ForceMode.Impulse);
    }
}
