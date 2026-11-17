using UnityEngine;

public class InteractiveDoor : MonoBehaviour
{
    private bool isPlayerNearby = false;
    private bool isDoorOpen = false;
    
    private MeshRenderer[] doorVisuals;
    private Collider[] allColliders;
    private Transform playerTransform;

    [Header("Interaction Settings")]
    [Tooltip("How close the player must be to open the door.")]
    public float interactionDistance = 3.0f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Audio Settings")]
    [Tooltip("Drag your door opening sound clip here.")]
    public AudioClip openSound;
    [Tooltip("Optional: Drag your door closing sound clip here.")]
    public AudioClip closeSound;

    private AudioSource audioSource;

    void Start()
    {
        // Set up AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        // Grab ALL MeshRenderers on this object and any child objects
        doorVisuals = GetComponentsInChildren<MeshRenderer>();

        // Grab ALL existing Colliders on this object and any child objects
        allColliders = GetComponentsInChildren<Collider>();

        // If the door model didn't come with a collider at all, create one automatically
        if (allColliders.Length == 0)
        {
            BoxCollider newCol = gameObject.AddComponent<BoxCollider>();
            allColliders = new Collider[] { newCol };
        }

        // Cache the player once at Start for better performance
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("InteractiveDoor: No object with the 'Player' tag was found in the scene!");
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Check distance between player and door
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        isPlayerNearby = (distance <= interactionDistance);

        if (isPlayerNearby && Input.GetKeyDown(interactKey))
        {
            ToggleDoor();
        }
    }

    void ToggleDoor()
    {
        isDoorOpen = !isDoorOpen;

        // Play open/close sound
        if (isDoorOpen && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }
        else if (!isDoorOpen && closeSound != null)
        {
            audioSource.PlayOneShot(closeSound);
        }

        // Hide the door meshes when open, show them when closed
        foreach (MeshRenderer renderer in doorVisuals)
        {
            if (renderer != null)
            {
                renderer.enabled = !isDoorOpen;
            }
        }

        // Disable the colliders completely when open so you can walk right through
        foreach (Collider col in allColliders)
        {
            if (col != null)
            {
                col.enabled = !isDoorOpen;
            }
        }
    }
}
