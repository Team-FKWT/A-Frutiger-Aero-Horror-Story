using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

using Cursor = UnityEngine.Cursor; //fixes ambiguous cursor value
public class KeycardTerminalController : MonoBehaviour
{
    [SerializeField] private PanelRenderer panelRenderer;
    [SerializeField] private KeycardUIController keycardUIController;

    private movementController playerMovement;
    private mouseController playerMouse;

    private bool isActive;
    private int openedFrame;

private void Start() //start disabled, only open when interacting w terminal
{
    if (panelRenderer != null)
        panelRenderer.enabled = false;
}
    public void OnInteract(object playerReference)
    {
        Debug.Log("KEYCARD TERMINAL OnInteract CALLED"); //temp, check called
        if (isActive)
            return;

        if (playerReference is not GameObject player)
            return;

        playerMovement = player.GetComponent<movementController>();
        playerMouse = player.GetComponent<mouseController>();

        isActive = true;
        openedFrame = Time.frameCount;

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (playerMouse != null)
            playerMouse.enabled = false;

        panelRenderer.enabled = true;

        if (keycardUIController != null)
        {
            keycardUIController.RefreshUI();
        }

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (!isActive)
            return;

        // Prevent the E that opened the panel from immediately closing it.
        if (Time.frameCount == openedFrame)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            DisengageInteract();
        }
    }

    public void DisengageInteract()
    {
        isActive = false;

        panelRenderer.enabled = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (playerMouse != null)
            playerMouse.enabled = true;
    }
}