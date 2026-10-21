using UnityEngine;
using UnityEngine.UIElements;

public class DesktopUIController : MonoBehaviour
{
    // TODO make button attributes generalised
    [SerializeField] public string itemName;
    [SerializeField] public Texture2D itemIcon;
    [SerializeField] public VisualTreeAsset itemContents;
    
    // Templates to instantiate
    [SerializeField] public VisualTreeAsset windowTemplate;
    [SerializeField] public VisualTreeAsset itemTemplate;
    
    // References to desktop UI elements
    private VisualElement desktopContentArea;
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
        desktopContentArea = root.Q<VisualElement>("DesktopContents");
        iconArea = desktopContentArea.Q<VisualElement>("Icons");
        // TODO make icon generation programmatic by collection
        var itemContainer = itemTemplate.Instantiate();
        var desktopItem = itemContainer.Q<DesktopItemElement>();
        iconArea.Add(desktopItem);
        desktopItem.Init(itemIcon, itemName, itemContents, desktopContentArea, windowTemplate);
    }
}
