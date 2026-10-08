using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class movementController : MonoBehaviour
{
    public Camera camera;

    private CharacterController controller;

    //Movement speeds for each state
    public float walkspeed = 15f;
    public float sprintSpeed = 22f;
    public float crouchSpeed = 7f;

    public float standingHeight = 2f;
    public float crouchingHeight = 1f;

    //lower camera during crouch
    public float crouchCameraOffset = -0.5f;

    //current speed applied to player
    public float speed;

    private Vector3 standingCenter;
    private Vector3 standingCameraPosition;
    private bool isCrouching;



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

        //save original dimensions & camera
        standingHeight = controller.height;
        standingCenter = controller.center;

        if (camera != null)
        {
            standingCameraPosition = camera.transform.localPosition;
        }

        speed = walkspeed;
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

        // Crouch function, lowers player and camera
        void Crouch()
        {
            
            if (isCrouching)//if already crouching, don't reapply settings
                return;

            isCrouching = true;

            float heightDifference = standingHeight - crouchingHeight;

            //reduce collision height
            controller.height = crouchingHeight;
            //keep player on ground
            controller.center = standingCenter - Vector3.up * (heightDifference / 2f);

            //lower viewpoint
            if (camera != null)
            {
                camera.transform.localPosition =
                    standingCameraPosition + Vector3.up * crouchCameraOffset;
            }
        }

        // Stand function, normally runs if crouch isn't
        void Stand()
        {
            if (!isCrouching) //do nothing if the player is already standing
                return;

            isCrouching = false;

            controller.height = standingHeight;
            controller.center = standingCenter;

            if (camera != null)
            {
                camera.transform.localPosition = standingCameraPosition;
            }
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

            //read crouch & sprint inputs
            bool wantsToCrouch = keyboard.leftCtrlKey.isPressed;
            bool wantsToSprint = keyboard.leftShiftKey.isPressed;

            if (wantsToCrouch)
            {
                Crouch();
            }
            else
            {
                Stand();
            }
            
            if (isCrouching)
            { //player will move at crouch speed
                speed = crouchSpeed;
            }
            else if (wantsToSprint && input.sqrMagnitude > 0f)
            {   //player will move at sprint speed
                speed = sprintSpeed;
            }
            else
            {
                speed = walkspeed;
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
