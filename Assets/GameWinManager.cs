using UnityEngine;

public class GameWinManager : MonoBehaviour
{
    // --- Public variables to link in the Inspector ---
    public GameObject objectToSpin;
    public GameObject winMessagePlane;
    public float spinSpeed = 90f;
    public AudioClip winSound;
    public AudioSource backgroundMusicSource; // New code: A slot for the background music

    // --- Private state variables ---
    private bool isGameWon = false;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isGameWon)
        {
            StartWinSequence();
        }
    }

    private void StartWinSequence()
    {
        isGameWon = true;

        if (winMessagePlane != null)
        {
            winMessagePlane.SetActive(true);
        }

        // New code: Stop the background music
        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.Stop();
        }

        if (winSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(winSound);
        }
    }

    void Update()
    {
        if (isGameWon && objectToSpin != null)
        {
            objectToSpin.transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
        }
    }
}