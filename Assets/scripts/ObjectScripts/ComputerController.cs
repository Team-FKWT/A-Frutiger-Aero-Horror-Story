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
    private InputAction interactEvent;
    
    // All Interactable-layer objects should implement OnInteract() to handle user interaction.
    public void OnInteract(object playerReference)
    {
        // Ensure playerReference is a GameObject!
        if (playerReference is not GameObject)
        {
            Debug.LogError("playerReference is not a GameObject! (received " +  playerReference.GetType().Name + ")");
            return;
        }
        player = (GameObject) playerReference;
        // Add listener for Interact (E) so player can back away from computer
        interactEvent = InputSystem.actions.FindAction("Interact");
        interactEvent.performed += DisengagePlayer;
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

    public void DisengagePlayer(InputAction.CallbackContext context)
    {
        // Lock cursor again
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Switch camera back
        playerCamera.enabled = true;
        closeupCamera.enabled = false;
        // Reenable controllers
        playerMovementController.enabled = true;
        playerMouseController.enabled = true;
        // Remove interact event handler
        interactEvent.performed -= DisengagePlayer;
    }
}