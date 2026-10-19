using UnityEngine;

// a stealth detector that only sees the player inside an invisible box that pans between points
public class stealth_camera : stealth_detector
{
    public Vector3[] panPoints = new Vector3[2];
    public Vector3 viewBoxSize = new Vector3(4f, 4f, 4f);
    public float panSpeed = 2f;
    public float pauseTime = 1f;

    private GameObject viewBox;
    private BoxCollider viewBoxCollider;
    private int targetIndex = 1;
    private int panDirection = 1;
    private float pauseTimer;


    protected override void Start()
    {
        if (panPoints.Length < 2)
        {
            Debug.LogError(name + ": stealth_camera needs at least 2 pan points");
            enabled = false;
            return;
        }

        // the box has a trigger collider but no renderer, so it's invisible
        viewBox = new GameObject(name + "_ViewBox");
        viewBox.transform.position = panPoints[0];
        viewBoxCollider = viewBox.AddComponent<BoxCollider>();
        viewBoxCollider.size = viewBoxSize;
        viewBoxCollider.isTrigger = true;

        base.Start();
    }

    void Update()
    {
        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;
        }
        else
        {
            // move toward the next point, then bounce back along the array (0 -> 1 -> 2 -> 1 -> 0 ...)
            viewBox.transform.position = Vector3.MoveTowards(viewBox.transform.position, panPoints[targetIndex], panSpeed * Time.deltaTime);

            if (viewBox.transform.position == panPoints[targetIndex])
            {
                if (targetIndex + panDirection < 0 || targetIndex + panDirection >= panPoints.Length)
                {
                    panDirection = -panDirection;
                }
                targetIndex += panDirection;
                pauseTimer = pauseTime;
            }
        }

        transform.LookAt(viewBox.transform);
    }

    protected override bool PlayerInRange()
    {
        Collider playerCollider = player.GetComponent<Collider>();
        if (playerCollider != null)
        {
            return viewBoxCollider.bounds.Intersects(playerCollider.bounds);
        }
        return viewBoxCollider.bounds.Contains(player.transform.position);
    }

    void OnDestroy()
    {
        if (viewBox != null)
        {
            Destroy(viewBox);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        for (int i = 0; i < panPoints.Length; i++)
        {
            Gizmos.DrawWireCube(panPoints[i], viewBoxSize);
            if (i > 0)
            {
                Gizmos.DrawLine(panPoints[i - 1], panPoints[i]);
            }
        }
    }
}
