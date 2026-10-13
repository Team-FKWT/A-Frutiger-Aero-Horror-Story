using UnityEngine;
using System.Collections;


public class stealth_detector : MonoBehaviour
{
    public float detectionRange;
    public float detectionRate;
    public float crouchMultiplier = 0.5f;

    public GameObject player;


    void Start()
    {
        StartCoroutine(OnPlayerDetected());
    }

    void Update()
    {
        
    }

    IEnumerator OnPlayerDetected()
    {
        while(true)
        {
            // this script slowly increases the detection level of the player when they are detected by the stealth detector
            if (Vector3.Distance(transform.position, player.transform.position) <= detectionRange)
            {
                if (player.GetComponent<movementController>().crouching)
                {
                    player.GetComponent<movementController>().detectionLevel += 1 * crouchMultiplier;
                }
                else
                {
                    player.GetComponent<movementController>().detectionLevel += 1;
                }
                Debug.Log("I SEE YOU" + player.GetComponent<movementController>().detectionLevel);
            } else
            {
                if (player.GetComponent<movementController>().crouching)
                {
                    player.GetComponent<movementController>().detectionLevel -= 1 * crouchMultiplier;
                }
                else
                {
                    player.GetComponent<movementController>().detectionLevel -= 1;
                }
                Debug.Log("i dont see you..." + player.GetComponent<movementController>().detectionLevel);
            }

            // this is what happens when the player is fully detected by the stealth detector, you can change this to whatever you want
            if (player.GetComponent<movementController>().detectionLevel > 100)
            {
                Debug.Log("ALERT!!");
            }
            yield return new WaitForSeconds(detectionRate);
        }
    }
}
