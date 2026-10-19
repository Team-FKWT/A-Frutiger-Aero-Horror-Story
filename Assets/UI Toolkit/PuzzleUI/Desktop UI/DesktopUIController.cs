using UnityEngine;
using UnityEngine.UIElements;

public class DesktopUIController : MonoBehaviour
{
    // TODO make button attributes generalised
    [SerializeField] public string itemName;
    [SerializeField] public Texture2D itemIcon;
    [SerializeField] public VisualTreeAsset itemWindow;
    [SerializeField] public VisualTreeAsset itemTemplate;
    
    // References to desktop UI elements
    private VisualElement contentArea;
    private VisualElement iconArea;
    
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
        // Find base elements of UI
        contentArea = root.Q<VisualElement>("DesktopContents");
        iconArea = contentArea.Q<VisualElement>("Icons");
        // TODO make icon generation programmatic by collection
        var itemContainer = itemTemplate.Instantiate();
        var desktopItem = itemContainer.Q<DesktopItemElement>();
        iconArea.Add(desktopItem);
        desktopItem.Init(itemIcon, itemName, itemWindow, contentArea);
    }
}
