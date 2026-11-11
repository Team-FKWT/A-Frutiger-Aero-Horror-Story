using UnityEngine;
using UnityEngine.InputSystem;

public class ComputerController : MonoBehaviour
{
    // Reference to camera for close-up computer view
    [SerializeField] public Camera closeupCamera;
    
    // Reference to player controller objects
    private Camera playerCamera;
    private movementController playerMovementController;
    private mouseController playerMouseController;
    private GameObject player;
    
    // Whether or not computer is engaged
    private bool isActive = false;
    // Frame the computer was opened on, so the same E press can't also close it - Adam
    private int openedFrame = -1;
    
    // All Interactable-layer objects should implement OnInteract() to handle user interaction.
    public void OnInteract(object playerReference)
    {
        // Ensure playerReference is a GameObject!
        if (playerReference is not GameObject)
        {
            Debug.LogError("playerReference is not a GameObject! (received " +  playerReference.GetType().Name + ")");
            return;
        }
        isActive = true;
        openedFrame = Time.frameCount;
        player = (GameObject) playerReference;
        // Get reference to camera of player object
        playerCamera = player.GetComponentInChildren<Camera>();
        playerMovementController = player.GetComponent<movementController>();
        playerMouseController = player.GetComponent<mouseController>();
        // Swap cameras
        closeupCamera.enabled = true;
        playerCamera.enabled = false;
        // Disable player movement & mouse control
        playerMovementController.enabled = false;
        playerMouseController.enabled = false;
        // Enable cursor
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        // Let the screen know the player sat down (e.g. a login box can take keyboard focus) - Adam
        BroadcastMessage("OnViewOpened", SendMessageOptions.DontRequireReceiver);
    }

    public void Update()
    {
        // If active, check for interact key to disengage (ignored while typing in a text box - Adam)
        if (isActive && Time.frameCount > openedFrame && !TextInputGuard.IsTyping && Keyboard.current.eKey.wasPressedThisFrame)
            DisengageInteract();
    }

    // Leaves the close-up view. Public so on-screen buttons (like a close X) can call it - Adam
    public void DisengageInteract()
    {
        if (!isActive) return;
        isActive = false;
        // Lock cursor again
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Switch camera back
        closeupCamera.enabled = false;
        playerCamera.enabled = true;
        // Reenable controllers
        playerMovementController.enabled = true;
        playerMouseController.enabled = true;
        // Let the screen know the player left (e.g. a login box gives up keyboard focus) - Adam
        BroadcastMessage("OnViewClosed", SendMessageOptions.DontRequireReceiver);
    }
}
