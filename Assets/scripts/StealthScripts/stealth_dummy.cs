using UnityEngine;

// a simple guard AI meant to go on a capsule. it walks between patrol points, stops at each one to look left and right,
// and if it sees the player (inside a camera-shaped view cone, limited by detectionRange) it stops to track them
public class stealth_dummy : stealth_detector
{
    public enum State { Patrolling, OnGuard, Alert }

    [Header("Patrol")]
    public Vector3[] patrolPoints = new Vector3[2];
    public bool loopPath = true; // true: 0 -> 1 -> 2 -> 0 ..., false: 0 -> 1 -> 2 -> 1 -> 0 ...
    public float moveSpeed = 2f;
    public float turnSpeed = 180f; 

    [Header("On Guard")]
    public float guardTime = 3f;
    public float guardLookAngle = 40f; 
    public float guardLookPeriod = 2f; 

    [Header("Alert")]
    public float alertTime = 3f; // how long it tracks the player after last seeing them
    public float postAlertGuardTime = 2f; // how long it stays on guard after losing the player before resuming

    public float fieldOfView = 60f; // vertical fov, like Camera.fieldOfView
    public float aspect = 2f;

    public State state = State.Patrolling;

    private int targetIndex;
    private int patrolDirection = 1;
    private float stateTimer;
    private float guardElapsed;
    private float guardBaseYaw;

    // what the dummy was doing before it got alerted, so it can resume afterwards
    private State resumeState;
    private float resumeTimer;
    private float resumeGuardElapsed;
    private float resumeGuardBaseYaw;
    private bool resumingAfterAlert;


    protected override void Start()
    {
        if (patrolPoints.Length == 0)
        {
            Debug.LogWarning(name + ": warning : dummy has no patrol points. it will be on guard only");
            StartGuard(guardTime);
        }

        base.Start();
    }

    void Update()
    {
        switch (state)
        {
            case State.Patrolling: UpdatePatrol(); break;
            case State.OnGuard: UpdateGuard(); break;
            case State.Alert: UpdateAlert(); break;
        }
    }

    void UpdatePatrol()
    {
        // move straight along the line to the point, including height, so it climbs/descends between points at different y levels
        Vector3 target = patrolPoints[targetIndex];

        TurnTowards(target);
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        if (transform.position == target)
        {
            StartGuard(guardTime);
        }
    }

    void UpdateGuard()
    {
        // sweep left and right around the direction it was facing when it stopped
        guardElapsed += Time.deltaTime;
        float yaw = guardBaseYaw + guardLookAngle * Mathf.Sin(guardElapsed * Mathf.PI * 2f / guardLookPeriod);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0f, yaw, 0f), turnSpeed * Time.deltaTime);

        stateTimer -= Time.deltaTime;
        if (stateTimer > 0f)
        {
            return;
        }

        if (resumingAfterAlert)
        {
            Resume();
        }
        else if (patrolPoints.Length > 0)
        {
            AdvancePatrolPoint();
            state = State.Patrolling;
        }
        else
        {
            StartGuard(guardTime);
        }
    }

    void UpdateAlert()
    {
        TurnTowards(player.transform.position);

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            // lost the player, look around for a bit before going back to what it was doing
            StartGuard(postAlertGuardTime);
            resumingAfterAlert = true;
        }
    }

    protected override void OnPlayerSeen()
    {
        if (state != State.Alert)
        {
            // only save what it was doing if it wasn't already in the post-alert look around
            if (!resumingAfterAlert)
            {
                resumeState = state;
                resumeTimer = stateTimer;
                resumeGuardElapsed = guardElapsed;
                resumeGuardBaseYaw = guardBaseYaw;
            }
            resumingAfterAlert = false;
            state = State.Alert;
            Debug.Log(name + ": spotted the player!");
        }

        // seeing the player again keeps it tracking for the full alert time
        stateTimer = alertTime;
    }

    void Resume()
    {
        resumingAfterAlert = false;
        state = resumeState;

        if (state == State.OnGuard)
        {
            stateTimer = resumeTimer;
            guardElapsed = resumeGuardElapsed;
            guardBaseYaw = resumeGuardBaseYaw;
        }
    }

    void StartGuard(float duration)
    {
        state = State.OnGuard;
        stateTimer = duration;
        guardElapsed = 0f;
        guardBaseYaw = transform.eulerAngles.y;
    }

    void AdvancePatrolPoint()
    {
        if (patrolPoints.Length < 2)
        {
            return;
        }

        if (loopPath)
        {
            targetIndex = (targetIndex + 1) % patrolPoints.Length;
            return;
        }

        if (targetIndex + patrolDirection < 0 || targetIndex + patrolDirection >= patrolPoints.Length)
        {
            patrolDirection = -patrolDirection;
        }
        targetIndex += patrolDirection;
    }

    // only turns around the y axis so the capsule stays upright
    void TurnTowards(Vector3 point)
    {
        Vector3 direction = point - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), turnSpeed * Time.deltaTime);
    }

    protected override bool PlayerInRange()
    {
        Vector3 local = transform.InverseTransformPoint(PlayerCenter());
        if (local.z <= 0f || local.z > detectionRange)
        {
            return false;
        }

        float halfHeight = Mathf.Tan(fieldOfView * 0.5f * Mathf.Deg2Rad) * local.z;
        float halfWidth = halfHeight * aspect;
        return Mathf.Abs(local.x) <= halfWidth && Mathf.Abs(local.y) <= halfHeight;
    }

    void OnDrawGizmos()
    {
        // patrol path
        Gizmos.color = Color.cyan;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            Gizmos.DrawWireSphere(patrolPoints[i], 0.25f);
            if (i > 0)
            {
                Gizmos.DrawLine(patrolPoints[i - 1], patrolPoints[i]);
            }
        }
        if (loopPath && patrolPoints.Length > 2)
        {
            Gizmos.DrawLine(patrolPoints[patrolPoints.Length - 1], patrolPoints[0]);
        }

        // line to the point it's currently heading for
        if (patrolPoints.Length > 0)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, patrolPoints[Mathf.Clamp(targetIndex, 0, patrolPoints.Length - 1)]);
        }

        // view frustum, coloured by what the dummy is doing
        switch (state)
        {
            case State.Patrolling: Gizmos.color = Color.green; break;
            case State.OnGuard: Gizmos.color = Color.yellow; break;
            case State.Alert: Gizmos.color = Color.red; break;
        }
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawFrustum(Vector3.zero, fieldOfView, detectionRange, 0f, aspect);

        // how far it looks left and right while on guard
        Gizmos.color = new Color(1f, 1f, 0f, 0.4f);
        Gizmos.DrawLine(Vector3.zero, Quaternion.Euler(0f, -guardLookAngle, 0f) * Vector3.forward * detectionRange);
        Gizmos.DrawLine(Vector3.zero, Quaternion.Euler(0f, guardLookAngle, 0f) * Vector3.forward * detectionRange);
        Gizmos.matrix = Matrix4x4.identity;
    }
}
