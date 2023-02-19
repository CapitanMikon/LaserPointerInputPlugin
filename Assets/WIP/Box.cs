using System;
using UnityEngine;

public class Box : MonoBehaviour, ILaserInteractable
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

    public void OnClick()
    {
        rb.useGravity = true;
        rb.AddExplosionForce(force, transform.position, 1f, 0f, ForceMode.Impulse);
    }
}
