using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class movementController : MonoBehaviour
{
    public Camera camera;

    private CharacterController controller;

    public float walkspeed = 15f;
    public float sprintSpeed = 22f;
    public float speed;

    public float gravity = 9.8f * 2f; //gravity in normal world, dream may be floaty ~9
    public float jumpHeight = 3f;

    public Transform groundDetect;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private Vector3 velocity;
    private bool onGround;

    public float detectionLevel = 0;

    public bool isDetected = false;

    public float crouchHeight = 1f;
    public float crouchSpeed = 6f;
    public float crouchTransitionSpeed = 10f;

    private CapsuleCollider capsule;
    private float standHeight;
    private Vector3 standCenter;
    private float capsuleStandHeight;
    private Vector3 capsuleStandCenter;
    private Vector3 cameraStandPosition;
    private float currentHeight;
    public bool crouching;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (speed <= 0f)
        {
            speed = walkspeed;
        }

        standHeight = controller.height;
        standCenter = controller.center;
        currentHeight = standHeight;

        // Optional extra capsule collider, kept in sync with the controller.
        capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            capsuleStandHeight = capsule.height;
            capsuleStandCenter = capsule.center;
        }

        crouchHeight = Mathf.Clamp(crouchHeight, controller.radius * 2f, standHeight);

        StartCoroutine(ResetDetection());

        if (camera == null)
        {
            camera = GetComponentInChildren<Camera>();
        }
        if (camera != null)
        {
            cameraStandPosition = camera.transform.localPosition;
        }
    }

    void Update()
    {
        // The controller also works when no ground probe or ground layer is configured.
        onGround = controller.isGrounded;
        if (groundDetect != null && groundMask.value != 0)
        {
            onGround |= Physics.CheckSphere(groundDetect.position, groundDistance,
                groundMask, QueryTriggerInteraction.Ignore);
        }

        if (onGround && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        Vector2 input = Vector2.zero;
        Keyboard keyboard = Keyboard.current;

        bool wantsSprint = false;
        
        if (keyboard != null)
        {
            // Ternary operators cut down on overhead.
            input.x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f)
                - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
            input.y = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f)
                - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);

            bool wantsCrouch = keyboard.leftCtrlKey.isPressed || keyboard.cKey.isPressed;
            crouching = wantsCrouch || (crouching && !CanStand());

            wantsSprint = keyboard.leftShiftKey.isPressed;

            if (keyboard.spaceKey.wasPressedThisFrame && onGround && !crouching)
            {
                velocity.y = Mathf.Sqrt(Mathf.Max(0f, jumpHeight * 2f * gravity));
            }    
        }

        UpdateCrouchHeight();

        input = Vector2.ClampMagnitude(input, 1f);
        Vector3 move = transform.right * input.x + transform.forward * input.y;
        velocity.y -= gravity * Time.deltaTime;
        float movementSpeed = speed;

        // Can't sprint while crouched ofc
            //(though i might add a slide later if u crouch while sprinting -frank)
        if (crouching)
        {
            movementSpeed = crouchSpeed;
        }
        else if (wantsSprint && input.sqrMagnitude > 0f)
        {
            movementSpeed = sprintSpeed;
        }

        CollisionFlags collisions =
            controller.Move((move * movementSpeed + velocity) * Time.deltaTime);

        if ((collisions & CollisionFlags.Above) != 0 && velocity.y > 0f)
        {
            velocity.y = 0f;
        }
    }

    // Checks for headroom between the crouched top and the standing top of the capsule.
    // If the player is not size 2 at standing and 1 at crouching, this functon breaks.
    // The radius of the character's collider can be decreased, but the height must remain consistent with the crouch and stand heights.
    // If the player's size must be mutable this must be adjusted. Otherwise it is adequate.  
    bool CanStand()
    {
        float radius = controller.radius * 0.95f;
        Vector3 bottom = transform.position + standCenter + Vector3.down * (standHeight * 0.5f - controller.radius);
        Vector3 start = bottom + Vector3.up * (currentHeight - controller.radius * 2f + controller.skinWidth);
        Vector3 top = bottom + Vector3.up * (standHeight - controller.radius * 2f + controller.skinWidth);

        foreach (Collider hit in Physics.OverlapCapsule(start, top, radius, ~0, QueryTriggerInteraction.Ignore))
        {
            if (!hit.transform.IsChildOf(transform))
            {
                return false;
            }
        }
        return true;
    }

    // Shrinks the capsule from the top so the feet stay planted, and lowers the camera with it.
    void UpdateCrouchHeight()
    {
        float targetHeight = crouching ? crouchHeight : standHeight;
        currentHeight = Mathf.MoveTowards(currentHeight, targetHeight,
            crouchTransitionSpeed * Time.deltaTime);

        float drop = standHeight - currentHeight;
        controller.height = currentHeight;
        controller.center = standCenter + Vector3.down * (drop * 0.5f);

        if (capsule != null)
        {
            capsule.height = capsuleStandHeight - drop;
            capsule.center = capsuleStandCenter + Vector3.down * (drop * 0.5f);
        }

        if (camera != null)
        {
            camera.transform.localPosition = cameraStandPosition + Vector3.down * drop;
        }
    }

    IEnumerator ResetDetection()
    {
        while (true)
        {
            if (!isDetected)
            {
                detectionLevel -= 2;
                detectionLevel = Mathf.Clamp(detectionLevel, 0, 100);
            }
            else
            {
                isDetected = false;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
}
