using UnityEngine;
using UnityEngine.InputSystem;

/// Debug fly mode for exploring scenes with blocked doors. Press F to toggle.
/// WASD to move where you look, Space up, Left Ctrl down, Left Shift faster.
/// Walls are ignored while flying. Turn it off (F) to walk and interact again.
public class FlyMode : MonoBehaviour
{
    [SerializeField] private Key toggleKey = Key.F;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float fastMultiplier = 2.5f;
    [SerializeField] private bool startFlying = false;

    private movementController movement;
    private CharacterController controller;
    private Camera cam;
    private bool flying;

    void Awake()
    {
        movement = GetComponent<movementController>();
        controller = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();
    }

    void Start()
    {
        if (startFlying) SetFlying(true);
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb[toggleKey].wasPressedThisFrame) SetFlying(!flying);
        if (!flying) return;

        // Move in the direction the camera faces
        Transform view = cam != null ? cam.transform : transform;
        Vector3 dir = Vector3.zero;
        if (kb.wKey.isPressed) dir += view.forward;
        if (kb.sKey.isPressed) dir -= view.forward;
        if (kb.dKey.isPressed) dir += view.right;
        if (kb.aKey.isPressed) dir -= view.right;
        if (kb.spaceKey.isPressed) dir += Vector3.up;
        if (kb.leftCtrlKey.isPressed) dir -= Vector3.up;

        float s = speed * (kb.leftShiftKey.isPressed ? fastMultiplier : 1f);
        transform.position += dir.normalized * s * Time.deltaTime;
    }

    void SetFlying(bool on)
    {
        flying = on;
        // Turning these off stops gravity and walking, and lets you pass through walls
        if (movement != null) movement.enabled = !on;
        if (controller != null) controller.enabled = !on;
    }
}