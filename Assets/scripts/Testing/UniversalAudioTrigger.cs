using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AudioSource))]
public class UniversalAudioTrigger : MonoBehaviour, IPointerClickHandler
{
    public enum MovementType { Rigidbody, CharacterController, TransformPosition }

    [Header("--- AUDIO CLIPS ---")]
    [Tooltip("Add one or more sounds here. If you add multiple, it will pick one at random (perfect for footsteps!).")]
    [SerializeField] private AudioClip[] audioClips;
    [Range(0f, 1f)] [SerializeField] private float volume = 1.0f;
    [Range(0.1f, 3f)] [SerializeField] private float minPitch = 0.95f;
    [Range(0.1f, 3f)] [SerializeField] private float maxPitch = 1.05f;

    [Header("--- CHOOSE ONE MODE BELOW ---")]
    
    [Header("[Mode 1: Player Footsteps]")]
    [SerializeField] private bool isPlayerFootsteps = false;
    [SerializeField] private MovementType movementComponent = MovementType.Rigidbody;
    [SerializeField] private float stepInterval = 0.4f;
    [SerializeField] private float speedThreshold = 0.1f;
    
    [Tooltip("CRITICAL: Turn this ON to stop sounds while jumping/falling!")]
    [SerializeField] private bool requireGroundedToStep = true;
    [Tooltip("What layer is your ground/floor set to? (Default is usually fine)")]
    [SerializeField] private LayerMask groundLayer = ~0; 

    [Header("[Mode 2: Bumping / Collision]")]
    [SerializeField] private bool playOnCollision = false;
    [Tooltip("If checked, it only plays when colliding with this specific tag.")]
    [SerializeField] private bool filterByTag = true;
    [SerializeField] private string targetTag = "Player";

    [Header("[Mode 3: Proximity Trigger (Walk Into Area)]")]
    [SerializeField] private bool playOnTriggerEnter = false;

    [Header("[Mode 4: Key Press Interaction (e.g. Press E to Open)]")]
    [SerializeField] private bool playOnKeyPress = false;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [Tooltip("Must be inside this trigger zone to press the key.")]
    [SerializeField] private bool requireProximityToPress = true;

    private AudioSource audioSource;
    private float stepTimer;
    private Rigidbody rb;
    private CharacterController cc;
    private Collider myCollider;
    private Vector3 lastPosition;
    private bool playerIsClose = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        cc = GetComponent<CharacterController>();
        myCollider = GetComponent<Collider>();
        lastPosition = transform.position;

        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(PlayAudio);
        }
    }

    private void Update()
    {
        if (isPlayerFootsteps)
        {
            HandleFootsteps();
        }

        if (playOnKeyPress && Input.GetKeyDown(interactionKey))
        {
            if (!requireProximityToPress || playerIsClose)
            {
                PlayAudio();
            }
        }
    }

    public void PlayAudio()
    {
        if (audioClips == null || audioClips.Length == 0) return;

        int randomIndex = Random.Range(0, audioClips.Length);
        AudioClip clipToPlay = audioClips[randomIndex];

        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(clipToPlay, volume);
    }

    private void HandleFootsteps()
    {
        // STEP 1: If we require the player to be grounded, check if they are in the air
        if (requireGroundedToStep)
        {
            if (!CheckIfGrounded())
            {
                stepTimer = 0f; // Reset timer so footsteps don't "buffer" mid-air
                return;         // EXIT immediately - do not play sound!
            }
        }

        // STEP 2: Calculate Speed
        float currentSpeed = 0f;

        if (movementComponent == MovementType.Rigidbody && rb != null)
        {
            currentSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
        }
        else if (movementComponent == MovementType.CharacterController && cc != null)
        {
            currentSpeed = new Vector3(cc.velocity.x, 0, cc.velocity.z).magnitude;
        }
        else 
        {
            Vector3 distanceMoved = transform.position - lastPosition;
            distanceMoved.y = 0; 
            currentSpeed = distanceMoved.magnitude / Time.deltaTime;
            lastPosition = transform.position;
        }

        // STEP 3: Play Sound if moving
        if (currentSpeed > speedThreshold)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayAudio();
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    private bool CheckIfGrounded()
    {
        // If using a Character Controller, use its built-in ground check
        if (movementComponent == MovementType.CharacterController && cc != null)
        {
            return cc.isGrounded;
        }

        // Fallback: Shoot a tiny laser ray downward from the bottom of the object to see if floor is there
        float rayLength = 0.3f; 
        Vector3 rayOrigin = transform.position;

        if (myCollider != null)
        {
            // Start the ray at the very bottom edge of your collider
            rayOrigin = new Vector3(myCollider.bounds.center.x, myCollider.bounds.min.y + 0.1f, myCollider.bounds.center.z);
        }

        // Returns true if it hits something on the floor layout
        return Physics.Raycast(rayOrigin, Vector3.down, rayLength, groundLayer);
    }

    // UI CLICK LOGIC
    public void OnPointerClick(PointerEventData eventData)
    {
        PlayAudio();
    }

    // PHYSICS COLLISION LOGIC
    private void OnCollisionEnter(Collision collision)
    {
        if (!playOnCollision) return;
        if (filterByTag && !collision.gameObject.CompareTag(targetTag)) return;

        PlayAudio();
    }

    // PROXIMITY ZONE LOGIC
    private void OnTriggerEnter(Collider other)
    {
        if (filterByTag && !other.CompareTag(targetTag)) return;

        if (playOnTriggerEnter)
        {
            PlayAudio();
        }
        
        if (other.CompareTag(targetTag))
        {
            playerIsClose = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            playerIsClose = false;
        }
    }
}
