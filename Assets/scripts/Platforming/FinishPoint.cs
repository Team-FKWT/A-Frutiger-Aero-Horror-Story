using UnityEngine;
using UnityEngine.UIElements;
public class FinishZone : MonoBehaviour
{
    public UIDocument uiDocument;

    private Label finishMessage;

    private void OnEnable()
    {
        finishMessage =
            uiDocument.rootVisualElement.Q<Label>("finish-message");

        finishMessage.style.display = DisplayStyle.None;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            finishMessage.style.display = DisplayStyle.Flex;
        }
    }
}
