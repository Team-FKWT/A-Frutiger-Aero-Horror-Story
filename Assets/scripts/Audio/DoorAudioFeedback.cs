using UnityEngine;

[RequireComponent(typeof(AudioCueEmitter), typeof(DoorController))]
public class DoorAudioFeedback : MonoBehaviour
{
    private DoorController door;
    private AudioCueEmitter emitter;
    private bool wasOpen;

    private void OnEnable()
    {
        door = GetComponent<DoorController>();
        emitter = GetComponent<AudioCueEmitter>();
        wasOpen = door.isOpen;
    }

    private void LateUpdate()
    {
        if (door.isOpen == wasOpen) return;
        wasOpen = door.isOpen;
        emitter.Play();
    }
}
