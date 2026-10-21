using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class ComboLockController : MonoBehaviour
{
    // Reference to UI Panel Renderer
    [SerializeField] public PanelRenderer panelRenderer;
    
    // Reference to player controller objects
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
            Debug.LogError("playerReference is not a GameObject! (received " + playerReference.GetType().Name + ")");
            return;
        }

        isActive = true;
        // Get reference to player controllers
        player = (GameObject) playerReference;
        playerMovementController = player.GetComponent<movementController>();
        playerMouseController = player.GetComponent<mouseController>();
        // Disable player movement & mouse control
        playerMovementController.enabled = false;
        playerMouseController.enabled = false;
        // Enable lock panel
        panelRenderer.enabled = true;
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
                // Disable panel renderer
                panelRenderer.enabled = false;
                // Lock cursor again
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                // Reenable controllers
                playerMovementController.enabled = true;
                playerMouseController.enabled = true;
            }
        }
    }
}