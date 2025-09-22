using UnityEngine;

/// <summary>
/// A simple script to make any GameObject spin continuously.
/// Attach this script to the object you want to rotate.
/// You can adjust the rotation speed and axis in the Inspector.
/// </summary>
public class SpinObject : MonoBehaviour
{
    // The speed at which the object will rotate.
    // You can set the X, Y, and Z values in the Unity Inspector
    // to control the rotation on each axis.
    [Tooltip("Defines the rotation speed on each axis (X, Y, Z).")]
    public Vector3 rotationSpeed = new Vector3(0, 50, 0);

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update()
    {
        // Rotate the object around its local axes.
        // We multiply by Time.deltaTime to make the rotation smooth and
        // independent of the frame rate. This means the object will rotate
        // at the same speed regardless of how fast the computer is.
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
