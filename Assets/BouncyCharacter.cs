using Unity.VisualScripting;
using UnityEngine;

public class BouncyCharacter : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float bounceFactor = 0.8f; // Controls the bounciness on impact
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Disable gravity to make the object float
        rb.useGravity = false;
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // Get horizontal input
        float moveX = Input.GetAxis("Horizontal");
        Vector3 moveDirection = new Vector3(moveX, 0, 0);

        // Apply movement
        rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, 0);

        // Jump (Apply vertical velocity manually if needed)
        if (Input.GetButtonDown("Jump"))
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("collision");

        // Apply bounce effect when hitting the ground or other surfaces
        if (collision.gameObject.CompareTag("Ground"))
        {
            Vector3 bounceVelocity = new Vector3(rb.linearVelocity.x, -rb.linearVelocity.y * bounceFactor, rb.linearVelocity.z);
            RootMetadata.velocity = bounceVelocity;
        }
    }
}