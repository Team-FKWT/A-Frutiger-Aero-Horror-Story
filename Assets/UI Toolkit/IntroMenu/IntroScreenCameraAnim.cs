using UnityEngine;

public class IntroScreenCameraAnim : MonoBehaviour
{
    [SerializeField] public GameObject tgtPoint;
    [SerializeField] public float moveTime = 3.0f;
    private float timeElapsed = 0f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (timeElapsed < moveTime)
        {
            transform.position = Vector3.Lerp(transform.position, tgtPoint.transform.position, timeElapsed / moveTime);
            timeElapsed += Time.deltaTime;
        }
    }
}
