using UnityEngine;

public class SimpleFirstPersonController : MonoBehaviour
{
    public float moveSpeed = 5f; // Movement speed
    public float lookSpeedX = 2f; // Mouse X rotation speed
    public float lookSpeedY = 2f; // Mouse Y rotation speed
    public float jumpForce = 5f; // Jump height
    public float gravity = -9.8f; // Gravity force

    private float rotationX = 0f; // Rotation on the X-axis (up/down)
    private float rotationY = 0f; // Rotation on the Y-axis (left/right)
    private CharacterController characterController;

    private Vector3 moveDirection = Vector3.zero;
    private Vector3 velocity; // This will store the velocity for gravity and jumping
    private Camera camera;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen
        Cursor.visible = false; // Hide the cursor
        camera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        // Mouse Look
        float mouseX = Input.GetAxis("Mouse X") * lookSpeedX;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeedY;

        // Rotate horizontally (left/right)
        rotationY += mouseX;

        // Rotate vertically (up/down), clamping the rotation to prevent flipping
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        // Apply the rotations
        transform.rotation = Quaternion.Euler(0, rotationY, 0); // Rotate the player body on the Y-axis
        camera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0); // Rotate the camera on the X-axis

        // Movement Input (WASD)
        float moveDirectionX = Input.GetAxis("Horizontal"); // A/D (left/right)
        float moveDirectionZ = Input.GetAxis("Vertical"); // W/S (forward/backward)

        Vector3 move = transform.right * moveDirectionX + transform.forward * moveDirectionZ;
        moveDirection = move * moveSpeed;

        // Jumping
        if (characterController.isGrounded)
        {
            if (Input.GetButtonDown("Jump")) // Spacebar press
            {
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity); // Calculate the jump force
            }
            else
            {
                velocity.y = -2f; // Small downward force to keep the character grounded
            }
        }
        else
        {
            velocity.y += gravity * Time.deltaTime; // Apply gravity if not grounded
        }

        // Apply movement and gravity
        characterController.Move((moveDirection + velocity) * Time.deltaTime);
    }
}
