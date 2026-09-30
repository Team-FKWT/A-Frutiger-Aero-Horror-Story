using UnityEngine;


public class stealth_detector : MonoBehaviour
{
    public bool isDetected;
    public float detectionRange;

    public Transform player;


    void Start()
    {
        isDetected = false;
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, player.position) <= detectionRange)
        {
            isDetected = true;
            Debug.Log("Player detected!");
        }
        else
        {
            isDetected = false;
        }
    }
}
