using UnityEngine;

public class SimpleCollisionTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log($"SimpleCollisionTest started on: {gameObject.name}");

        Collider col = GetComponent<Collider>();
        Debug.Log($"Collider found: {col != null}");

        if (col != null)
        {
            Debug.Log($"Collider type: {col.GetType().Name}");
            Debug.Log($"IsTrigger: {col.isTrigger}");
            Debug.Log($"Enabled: {col.enabled}");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"COLLISION! Hit by: {collision.gameObject.name} (Tag: {collision.gameObject.tag})");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"TRIGGER! Hit by: {other.gameObject.name} (Tag: {other.gameObject.tag})");
    }
}