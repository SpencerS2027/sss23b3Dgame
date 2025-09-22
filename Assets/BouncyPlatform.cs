using UnityEngine;

public class BouncyPlatform : MonoBehaviour
{
    [Header("Bounce Settings")]
    [SerializeField] private float bounceForce = 15f;
    [SerializeField] private bool useUpwardForceOnly = true;
    [SerializeField] private float bounceMultiplier = 1.2f;

    [Header("Audio & Effects (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip bounceSound;
    [SerializeField] private ParticleSystem bounceEffect;

    [Header("Bounce Cooldown")]
    [SerializeField] private float bounceCooldown = 0.5f;
    private float lastBounceTime;

    private void Start()
    {
        Debug.Log($"=== BouncyPlatform Detailed Diagnostics on: {gameObject.name} ===");

        // Direct component access test
        Debug.Log("=== DIRECT ACCESS TESTS ===");
        try
        {
            BoxCollider directBoxCollider = gameObject.GetComponent<BoxCollider>();
            Debug.Log($"Direct BoxCollider access: {directBoxCollider != null}");
            if (directBoxCollider != null)
            {
                Debug.Log($"  Direct BoxCollider - Enabled: {directBoxCollider.enabled}, IsTrigger: {directBoxCollider.isTrigger}");
                Debug.Log($"  Direct BoxCollider - Size: {directBoxCollider.size}, Center: {directBoxCollider.center}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Exception accessing BoxCollider directly: {e.Message}");
        }

        try
        {
            BoxCollider directCollider = gameObject.GetComponent<BoxCollider>();
            Debug.Log($"Direct BoxCollider access: {directCollider != null}");
            if (directCollider != null)
            {
                Debug.Log($"  Direct BoxCollider type: {directCollider.GetType().Name}");
                Debug.Log($"  Direct BoxCollider - Enabled: {directCollider.enabled}, IsTrigger: {directCollider.isTrigger}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Exception accessing BoxCollider directly: {e.Message}");
        }

        // Alternative access methods
        Debug.Log("=== ALTERNATIVE ACCESS METHODS ===");
        Collider[] allColliders = gameObject.GetComponents<Collider>();
        Debug.Log($"GetComponents<Collider> array length: {allColliders.Length}");
        for (int i = 0; i < allColliders.Length; i++)
        {
            if (allColliders[i] != null)
                Debug.Log($"  Collider[{i}]: {allColliders[i].GetType().Name}");
            else
                Debug.Log($"  Collider[{i}]: NULL");
        }

        // Component counting (safer method)
        Debug.Log("=== COMPONENT ANALYSIS ===");
        Component[] allComponents = gameObject.GetComponents<Component>();
        int nullCount = 0;
        int validCount = 0;

        for (int i = 0; i < allComponents.Length; i++)
        {
            if (allComponents[i] == null)
            {
                nullCount++;
                Debug.LogWarning($"  Component[{i}]: NULL COMPONENT");
            }
            else
            {
                validCount++;
                Debug.Log($"  Component[{i}]: {allComponents[i].GetType().Name}");
            }
        }

        Debug.Log($"Valid components: {validCount}, NULL components: {nullCount}");

        // Final collider setup - try BoxCollider specifically first
        BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            boxCollider.isTrigger = false;
            Debug.Log($"SUCCESS: BoxCollider found and configured!");
        }
        else
        {
            // Fallback to generic Collider
            Collider workingCollider = gameObject.GetComponent<Collider>();
            if (workingCollider != null)
            {
                workingCollider.isTrigger = false;
                Debug.Log($"SUCCESS: Generic Collider found and configured!");
            }
            else
            {
                Debug.LogError($"FAILED: Still no collider detected after all tests");
                // Don't add a new one yet - let's see what the diagnostics show first
            }
        }

        // Player check
        GameObject player = GameObject.FindWithTag("Player");
        Debug.Log($"Player tagged object found: {player != null}");
        if (player != null)
        {
            Debug.Log($"Player name: {player.name}");
            Debug.Log($"Player has Rigidbody: {player.GetComponent<Rigidbody>() != null}");
            Debug.Log($"Player has CharacterController: {player.GetComponent<CharacterController>() != null}");
        }

        Debug.Log($"=== End Diagnostics ===");
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"BouncyPlatform: Collision detected with {collision.gameObject.name}, Tag: {collision.gameObject.tag}");

        // Check if the colliding object is the player OR has a player parent
        GameObject playerObject = FindPlayerObject(collision.gameObject);
        if (playerObject != null)
        {
            Debug.Log($"Player found: {playerObject.name} - calling HandleBounce");
            HandleBounce(playerObject, collision.contacts[0].normal);
        }
        else
        {
            Debug.Log($"No Player found in collision hierarchy");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"BouncyPlatform: Trigger detected with {other.gameObject.name}, Tag: {other.gameObject.tag}");

        // Alternative trigger-based collision detection
        GameObject playerObject = FindPlayerObject(other.gameObject);
        if (playerObject != null)
        {
            Debug.Log($"Player found: {playerObject.name} - calling HandleBounce");
            // Calculate approximate normal (upward for suspended platforms)
            Vector3 normal = Vector3.up;
            HandleBounce(playerObject, normal);
        }
        else
        {
            Debug.Log($"No Player found in trigger hierarchy");
        }
    }

    private GameObject FindPlayerObject(GameObject collisionObject)
    {
        // Check if this object is tagged as Player
        if (collisionObject.CompareTag("Player"))
        {
            return collisionObject;
        }

        // Check parent objects for Player tag
        Transform current = collisionObject.transform;
        while (current != null)
        {
            if (current.CompareTag("Player"))
            {
                return current.gameObject;
            }
            current = current.parent;
        }

        // Check child objects for Player tag
        return FindPlayerInChildren(collisionObject.transform);
    }

    private GameObject FindPlayerInChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag("Player"))
            {
                return child.gameObject;
            }

            GameObject foundInGrandchildren = FindPlayerInChildren(child);
            if (foundInGrandchildren != null)
            {
                return foundInGrandchildren;
            }
        }
        return null;
    }

    private void HandleBounce(GameObject player, Vector3 surfaceNormal)
    {
        Debug.Log($"HandleBounce called for {player.name}");

        // Check cooldown to prevent rapid multiple bounces
        if (Time.time - lastBounceTime < bounceCooldown)
        {
            Debug.Log("Bounce blocked by cooldown");
            return;
        }

        lastBounceTime = Time.time;
        Debug.Log("Bounce cooldown passed - proceeding with bounce");

        // Try to get CharacterController first
        CharacterController characterController = player.GetComponent<CharacterController>();
        if (characterController != null)
        {
            Debug.Log("Using CharacterController bounce method");
            HandleCharacterControllerBounce(player, surfaceNormal);
            return;
        }

        // Try to get Rigidbody
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            Debug.Log("Using Rigidbody bounce method");
            HandleRigidbodyBounce(playerRb, surfaceNormal);
            return;
        }

        Debug.LogWarning("BouncyPlatform: Player object has neither CharacterController nor Rigidbody!");
    }

    private void HandleCharacterControllerBounce(GameObject player, Vector3 surfaceNormal)
    {
        // For CharacterController, we need to apply the bounce through the player's movement script
        // This assumes the player has a script that can receive bounce velocity

        // Try to find a common player movement script interface
        var playerMovement = player.GetComponent<IBounceable>();
        if (playerMovement != null)
        {
            Vector3 bounceVelocity = CalculateBounceVelocity(surfaceNormal);
            playerMovement.ApplyBounce(bounceVelocity);
        }
        else
        {
            // Fallback: Try to find any MonoBehaviour with a public method to handle bounce
            var movementScript = player.GetComponent<MonoBehaviour>();
            if (movementScript != null)
            {
                // You can customize this to match your player controller's method name
                Vector3 bounceVelocity = CalculateBounceVelocity(surfaceNormal);

                // Try to invoke a bounce method if it exists
                var bounceMethod = movementScript.GetType().GetMethod("ApplyBounce");
                if (bounceMethod != null)
                {
                    bounceMethod.Invoke(movementScript, new object[] { bounceVelocity });
                }
                else
                {
                    Debug.Log($"BouncyPlatform: Applied bounce to {player.name} but no ApplyBounce method found.");
                }
            }
        }

        PlayBounceEffects();
    }

    private void HandleRigidbodyBounce(Rigidbody playerRb, Vector3 surfaceNormal)
    {
        Vector3 bounceVelocity = CalculateBounceVelocity(surfaceNormal);

        if (useUpwardForceOnly)
        {
            // Reset downward velocity and apply upward bounce
            Vector3 velocity = playerRb.linearVelocity;
            velocity.y = Mathf.Max(0, velocity.y); // Remove downward velocity
            velocity.y += bounceVelocity.y;
            playerRb.linearVelocity = velocity;
        }
        else
        {
            // Apply full bounce velocity
            playerRb.AddForce(bounceVelocity, ForceMode.VelocityChange);
        }

        PlayBounceEffects();
    }

    private Vector3 CalculateBounceVelocity(Vector3 surfaceNormal)
    {
        if (useUpwardForceOnly)
        {
            return Vector3.up * bounceForce;
        }
        else
        {
            return surfaceNormal * bounceForce * bounceMultiplier;
        }
    }

    private void PlayBounceEffects()
    {
        // Play sound effect
        if (audioSource && bounceSound)
        {
            audioSource.PlayOneShot(bounceSound);
        }

        // Play particle effect
        if (bounceEffect)
        {
            bounceEffect.Play();
        }

        // Optional: Add a small screen shake or platform animation here
        StartCoroutine(PlatformBounceAnimation());
    }

    private System.Collections.IEnumerator PlatformBounceAnimation()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 squashScale = new Vector3(originalScale.x * 1.1f, originalScale.y * 0.9f, originalScale.z * 1.1f);

        // Squash
        float elapsed = 0f;
        float duration = 0.1f;
        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(originalScale, squashScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Stretch back
        elapsed = 0f;
        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(squashScale, originalScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
    }
}

// Interface for player controllers to implement bounce functionality
public interface IBounceable
{
    void ApplyBounce(Vector3 bounceVelocity);
}