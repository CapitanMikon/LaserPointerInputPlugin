using System;
using UnityEngine;

public class Box : MonoBehaviour, LPIPIInteractable
{
    private Rigidbody rb;

    [SerializeField] private float force;

    private Vector3 startingPos;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startingPos = transform.position;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.position = startingPos;
        }
    }

    public void LPIPOnLaserHit()
    {
        rb.useGravity = true;
        rb.AddForce(Vector3.Normalize(transform.position - Camera.main.transform.position) * force);
        
        //doesnt work properly
        //rb.AddExplosionForce(force, transform.position + Vector3.Normalize(transform.position + Camera.main.transform.position), 1f, 0f, ForceMode.Impulse);
    }
    

    public void ResetBox()
    {
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = startingPos;
    }
}
