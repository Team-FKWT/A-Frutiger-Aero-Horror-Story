using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


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
                OnPlayerSeen();
            }

            // this is what happens when the player is fully detected by the stealth detector, you can change this to whatever you want
            if (playerController.detectionLevel >= 100)
            {
                playerController.detectionLevel = 0;
                SceneManager.LoadScene("StealthScene");
            }
            yield return new WaitForSeconds(detectionRate);
        }
    }

    // called every detection tick the player is seen, subclasses can override this to react (e.g. a guard going on alert)
    protected virtual void OnPlayerSeen() { }

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
        RaycastHit[] hits = Physics.RaycastAll(transform.position, direction.normalized, direction.magnitude, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            // skip the detector's own colliders (e.g. the camera lens) so it doesn't block its own view
            if (hit.transform.IsChildOf(transform))
            {
                continue;
            }

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
