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
        
    }

    public void Update()
    {
        // If active, check for interact key to disengage
        if (isActive)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
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
            }
        }
    }
}