using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class InteractiveViewController : MonoBehaviour
{
    // Reference to UI Panel Renderer
    [SerializeField] public PanelRenderer panelRenderer;

    // Reference to player controller objects
    private movementController playerMovementController;
    private mouseController playerMouseController;
    private GameObject player;

    // Whether or not computer is engaged
    private bool isActive = false;
    // Frame the panel was opened on, so the same E press can't also close it - Adam
    private int openedFrame = -1;

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
        openedFrame = Time.frameCount;
        // Get reference to player controllers
        player = (GameObject)playerReference;
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
        // (not on the frame it opened, and not while typing in a text box - Adam)
        if (!isActive || Time.frameCount <= openedFrame) return;
        // Esc always closes; E closes unless typing - Adam
        if (Keyboard.current.escapeKey.wasPressedThisFrame ||
            (!TextInputGuard.IsTyping && Keyboard.current.eKey.wasPressedThisFrame))
            DisengageInteract();
    }

    public void DisengageInteract()
    {
        if (!isActive) return; // already closed - Adam
        isActive = false;
        // Disable panel renderer
        panelRenderer.enabled = false;
        // Lock cursor again
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Reenable controllers next frame, so the Esc that closed this doesn't also open the pause menu - Adam
        // Runs on the player, so it still finishes if this object is hidden right after closing
        playerMovementController.StartCoroutine(ReenableControlsNextFrame());
    }

    private IEnumerator ReenableControlsNextFrame()
    {
        yield return null;
        playerMovementController.enabled = true;
        playerMouseController.enabled = true;
    }
}