using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class movementController : MonoBehaviour
{
    public Camera camera;

    private CharacterController controller;

    public float walkspeed = 15f;
    public float speed;

    public float gravity = 9.8f * 2f;
    public float jumpHeight = 4f;

    public Transform groundDetect;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private Vector3 velocity;
    private bool onGround;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (speed <= 0f)
        {
            speed = walkspeed;
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
        if (keyboard != null)
        {
            input.x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f)
                - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
            input.y = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f)
                - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);

            if (keyboard.spaceKey.wasPressedThisFrame && onGround)
            {
                velocity.y = Mathf.Sqrt(Mathf.Max(0f, jumpHeight * 2f * gravity));
            }
        }

        input = Vector2.ClampMagnitude(input, 1f);
        Vector3 move = transform.right * input.x + transform.forward * input.y;
        velocity.y -= gravity * Time.deltaTime;
        CollisionFlags collisions = controller.Move((move * speed + velocity) * Time.deltaTime);

        if ((collisions & CollisionFlags.Above) != 0 && velocity.y > 0f)
        {
            velocity.y = 0f;
        }
    }
}
