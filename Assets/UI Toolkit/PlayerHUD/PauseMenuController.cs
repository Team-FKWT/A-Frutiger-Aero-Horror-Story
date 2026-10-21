using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class PauseMenuController : MonoBehaviour
{
    private Button exitButton;
    private Button resumeButton;
    
    private movementController movementController;
    private mouseController mouseController;

    [SerializeField] public string introScene;
    
    void Awake()
    {
        GetComponent<PanelRenderer>().RegisterUIReloadCallback(OnUIReload);
    }

    void OnDestroy()
    {
        GetComponent<PanelRenderer>().UnregisterUIReloadCallback(OnUIReload);
    }

    void OnEnable()
    {
        // Take over movement and mouse control
        movementController = gameObject.GetComponentInParent<movementController>();
        mouseController = gameObject.GetComponentInParent<mouseController>();
        movementController.enabled = false;
        mouseController.enabled = false;
        // Enable cursor
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    private void OnUIReload(PanelRenderer panelRenderer, VisualElement root, int version)
    {
        exitButton = root.Q<Button>("ExitButton");
        resumeButton = root.Q<Button>("ResumeButton");
        // When exit is clicked, load IntroMenu scene
        exitButton.clicked += () =>
        {
            // Check that the specified scene exists
            if (Application.CanStreamedLevelBeLoaded(introScene))
                SceneManager.LoadScene(introScene);
            else
                Debug.Log("Couldn't load intro menu scene (" + introScene + ")");
        };
        // When resume button is clicked, dismiss pause menu
        resumeButton.clicked += () =>
        {
            // Relinquish movement and mouse control
            gameObject.SetActive(false);
            movementController.enabled = true;
            mouseController.enabled = true;
            // Capture cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        };
    }
}
