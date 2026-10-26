using UnityEngine;

public class DoorController : MonoBehaviour
{
    // Instance field to track door state
    [SerializeField] public bool isLocked = false;
    [SerializeField] public bool isOpen = false;
    
    // Reference to GameObject to disable
    // TODO actually rotate/move the door
    [SerializeField] public GameObject doorObject;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // If door is supposed to be open and object is still active...
        if (isOpen && doorObject.activeSelf)
        {
            // This is where the door would get opened
            doorObject.SetActive(false);
        }
        // If door is supposed to be closed and object is inactive
        else if (!isOpen && !doorObject.activeSelf)
        {
            doorObject.SetActive(true);
        }
    }
    // All Interactable-layer objects should implement OnInteract() to handle user interaction.
    public void OnInteract(object playerReference)
    {
        if (!isLocked)
            {
            isOpen = !isOpen;
            }
    }
    
    // Method to unlock door for lock mechanisms to manipulate
    public void UnlockDoor()
    {
        isLocked = false;
    }

    public void LockDoor()
    {
        isLocked = true;
    }
}
