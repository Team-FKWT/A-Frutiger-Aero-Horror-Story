using UnityEngine;
using System.Collections;


public class stealth_detector : MonoBehaviour
{
    public float detectionRange;
    public float detectionRate;
    public float detectionAmount;
    public float crouchMultiplier = 0.5f;

    public GameObject player;

    protected movementController playerController;


    protected virtual void Start()
    {
        playerController = player.GetComponent<movementController>();
        StartCoroutine(OnPlayerDetected());
    }

    IEnumerator OnPlayerDetected()
    {
        while(true)
        {
            // this script slowly increases the detection level of the player when they are detected by the stealth detector
            if (PlayerInRange() && HasLineOfSight())
            {
                playerController.isDetected = true; // Initialize detection increment
                if (playerController.crouching)
                {
                    playerController.detectionLevel += detectionAmount * crouchMultiplier;
                }
                else
                {
                    playerController.detectionLevel += detectionAmount;
                }
                playerController.detectionLevel = Mathf.Clamp(playerController.detectionLevel, 0, 100);
                Debug.Log("I SEE YOU" + playerController.detectionLevel);
            } 

            // this is what happens when the player is fully detected by the stealth detector, you can change this to whatever you want
            if (playerController.detectionLevel >= 100)
            {
                Debug.Log("ALERT!!");
            }
            yield return new WaitForSeconds(detectionRate);
        }
    }

    // proximity detectors use a simple radius, cameras override this with their view box
    protected virtual bool PlayerInRange()
    {
        return Vector3.Distance(transform.position, player.transform.position) <= detectionRange;
    }

    // shoots a ray at the player, if it hits anything that isn't the player then the view is blocked
    protected bool HasLineOfSight()
    {
        Vector3 target = PlayerCenter();
        Vector3 direction = target - transform.position;

        // triggers are ignored so the camera's view box (and other trigger volumes) don't block the ray
        if (Physics.Raycast(transform.position, direction.normalized, out RaycastHit hit, direction.magnitude, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            bool hitPlayer = hit.transform == player.transform || hit.transform.IsChildOf(player.transform);
            Debug.DrawLine(transform.position, hit.point, hitPlayer ? Color.red : Color.yellow, detectionRate);
            return hitPlayer;
        }

        Debug.DrawLine(transform.position, target, Color.red, detectionRate);
        return true;
    }

    // aim at the middle of the player's body instead of their feet so the ray doesn't clip the floor
    protected Vector3 PlayerCenter()
    {
        Collider playerCollider = player.GetComponent<Collider>();
        return playerCollider != null ? playerCollider.bounds.center : player.transform.position;
    }
}
