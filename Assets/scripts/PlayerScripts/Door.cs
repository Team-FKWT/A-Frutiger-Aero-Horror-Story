using UnityEngine;

public class InteractiveDoor : MonoBehaviour
{
    private bool isPlayerNearby = false;
    private bool isDoorOpen = false;
    
    private MeshRenderer doorVisual;
    private BoxCollider doorCollider; // The single consolidated collider

    [Header("Audio Settings")]
    [Tooltip("Drag your door opening sound clip here.")]
    public AudioClip openSound;
    [Tooltip("Optional: Drag your door closing sound clip here.")]
    public AudioClip closeSound;

    private AudioSource audioSource;
    
    // Store original sizes to calculate open/closed state modifications
    private Vector3 closedSize;
    private Vector3 closedCenter;

    void Start()
    {
        // Automatically set up an AudioSource component
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.playOnAwake = false;

        // Get the visual mesh component of this door model
        doorVisual = GetComponentInChildren<MeshRenderer>();
        if (doorVisual == null) 
        {
            Debug.LogError("InteractiveDoor: No MeshRenderer found in children! Please attach one.");
            return; 
        }

        // Measure the child model's exact bounds
        Bounds bounds = doorVisual.bounds;
        Vector3 parentLocalSize = transform.InverseTransformVector(bounds.size);

        // Create our SINGLE BoxCollider. It starts as a solid blocking wall.
        doorCollider = gameObject.AddComponent<BoxCollider>();
        doorCollider.size = new Vector3(parentLocalSize.x, parentLocalSize.y, parentLocalSize.z);
        doorCollider.center = Vector3.zero;
        doorCollider.isTrigger = false; // Solid physics block when closed

        // Cache the closed dimensions for resetting later
        closedSize = doorCollider.size;
        closedCenter = doorCollider.center;
    }

    void Update()
    {
        // Because the collider is solid when closed, we use a small distance or overlap check 
        // to see if the player is standing in front of the door since OnTriggerEnter won't fire for solid objects.
        CheckPlayerProximity();

        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            ToggleDoor();
        }
    }

    void CheckPlayerProximity()
    {
        // Quick distance check from the door to find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // If player is within 2.5 units of the door center, they are nearby
        float distance = Vector3.Distance(transform.position, player.transform.position);
        isPlayerNearby = (distance <= 2.5f);
    }

    void ToggleDoor()
    {
        isDoorOpen = !isDoorOpen; 

        // Play audio safely
        if (isDoorOpen && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }
        else if (!isDoorOpen && closeSound != null)
        {
            audioSource.PlayOneShot(closeSound);
        }

        // Toggle visual rendering of the door mesh
        if (doorVisual != null) doorVisual.enabled = !isDoorOpen;

        // Update the single collider shape so it doesn't block the hallway
        UpdateColliderShape();
    }

    void UpdateColliderShape()
    {
        if (doorCollider == null) return;

        if (isDoorOpen)
        {
            // WHEN OPEN: Shrink the collider to just one of the frame edges (the "hinge" wall side)
            // This leaves the doorway completely hollow so you can walk right through.
            doorCollider.size = new Vector3(closedSize.x * 0.1f, closedSize.y, closedSize.z);
            doorCollider.center = new Vector3(-closedSize.x * 0.45f, closedCenter.y, closedCenter.z);
            doorCollider.isTrigger = false; // Keeps the frame edge solid, but clears the walkway
        }
        else
        {
            // WHEN CLOSED: Reset back to a full solid wall covering the entire gap
            doorCollider.size = closedSize;
            doorCollider.center = closedCenter;
            doorCollider.isTrigger = false;
        }
    }
}
