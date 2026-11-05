using UnityEngine;

public class InteractiveDoor : MonoBehaviour
{
    private bool isPlayerNearby = false;
    private bool isDoorOpen = false;
    
    private MeshRenderer doorVisual;
    private BoxCollider solidWall;
    private BoxCollider interactionTrigger;

    void Start()
    {
        // 1. Get the visual mesh component of this door model
        doorVisual = GetComponentInChildren<MeshRenderer>();
        if (doorVisual == null) return; 

        // Measure the model's exact 3D size and convert to local space
        Bounds bounds = doorVisual.bounds;
        Vector3 localSize = transform.InverseTransformVector(bounds.size);

        // 2. Automatically build the SOLID WALL collider
        solidWall = gameObject.AddComponent<BoxCollider>();
        solidWall.size = localSize;
        solidWall.isTrigger = false; 

        // 3. Automatically build the INTERACTION HITBOX trigger zone
        interactionTrigger = gameObject.AddComponent<BoxCollider>();
        interactionTrigger.size = new Vector3(localSize.x * 1.2f, localSize.y, localSize.z + 2.0f);
        interactionTrigger.isTrigger = true; 
    }

    void Update()
    {
        // If the player is standing inside this specific door's hitbox and presses E
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            ToggleDoor();
        }
    }

    void ToggleDoor()
    {
        isDoorOpen = !isDoorOpen; // Flip between open and closed

        // Turn the visuals on or off
        if (doorVisual != null) doorVisual.enabled = !isDoorOpen;

        // Turn the physical blocking wall on or off
        if (solidWall != null) solidWall.enabled = !isDoorOpen;
    }

    // Detect when the player enters this specific door's radar zone
    private void OnTriggerEnter(Collider other)
    {
        // Note: Make sure your player object has the tag "Player" set in Unity!
        if (other.CompareTag("Player") || other.gameObject.name.Contains("Player"))
        {
            isPlayerNearby = true;
        }
    }

    // Detect when the player leaves this specific door's radar zone
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.gameObject.name.Contains("Player"))
        {
            isPlayerNearby = false;
        }
    }
}
