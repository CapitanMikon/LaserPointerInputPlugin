using UnityEngine;

public class Box : MonoBehaviour
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

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.position = startingPos;
        }
    }

    private void OnMouseDown()
    {
        rb.useGravity = true;
        rb.AddExplosionForce(force, transform.position, 1f, 0f, ForceMode.Impulse);
    }
}
