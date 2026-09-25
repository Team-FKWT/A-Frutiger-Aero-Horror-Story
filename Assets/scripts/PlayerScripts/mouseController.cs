using UnityEngine;
using UnityEngine.InputSystem;

//test comment

public class mouseController : MonoBehaviour
{
    public float sensitivity = 100f;

    public Transform playerBody;
    public Transform cameraTransform;

    private float xRotation;

    void Start()
    {
        // Supports this script on either the player or its child camera.
        if (playerBody == null)
        {
            CharacterController controller = GetComponentInParent<CharacterController>();
            playerBody = controller != null ? controller.transform : transform;
        }

        if (cameraTransform == null)
        {
            Camera playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera != null)
            {
                cameraTransform = playerCamera.transform;
            }
        }

        if (cameraTransform != null && cameraTransform != playerBody)
        {
            xRotation = Mathf.DeltaAngle(0f, cameraTransform.localEulerAngles.x);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || Cursor.lockState != CursorLockMode.Locked || Time.timeScale == 0f)
        {
            return;
        }

        // Mouse delta is already the distance moved this frame: don't multiply by deltaTime.
        Vector2 look = mouse.delta.ReadValue() * (sensitivity * 0.001f);
        playerBody.Rotate(Vector3.up, look.x, Space.World);

        if (cameraTransform != null && cameraTransform != playerBody)
        {
            xRotation = Mathf.Clamp(xRotation - look.y, -90f, 90f);
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }
}
