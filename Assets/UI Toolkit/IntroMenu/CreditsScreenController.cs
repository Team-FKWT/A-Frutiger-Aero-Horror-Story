using UnityEngine;
using UnityEngine.UIElements;

public class CreditsScreenController : MonoBehaviour
{
    // Reference to main Intro Menu renderer
    [SerializeField] public PanelRenderer introScreenRenderer;

    private Button backButton;
    
    void Awake()
    {
        GetComponent<PanelRenderer>().RegisterUIReloadCallback(OnUIReload);
    }

    void OnDestroy()
    {
        GetComponent<PanelRenderer>().UnregisterUIReloadCallback(OnUIReload);
    }

    private void OnUIReload(PanelRenderer panelRenderer, VisualElement root, int version)
    {
        backButton = root.Q<Button>("BackButton");
        backButton.clicked += () =>
        {
            introScreenRenderer.GetComponent<PanelRenderer>().enabled = true;
            panelRenderer.enabled = false;
        };
    }
}
