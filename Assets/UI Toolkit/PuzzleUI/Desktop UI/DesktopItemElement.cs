using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class DesktopItemElement: VisualElement
{
    // References to item attributes
    private VisualElement itemIcon => this.Q<VisualElement>("ItemIcon");
    private Label itemLabel => this.Q<Label>("ItemLabel");
    private Button itemButton => this.Q<Button>("DesktopItem");
    
    // Window to open when item clicked.
    private VisualTreeAsset itemWindow;
    
    // Reference to parent tree to add window to.
    private VisualElement desktopContents;

    public void Init(Texture2D icon, string label, VisualTreeAsset window, VisualElement contentArea)
    {
        desktopContents = contentArea;
        itemIcon.style.backgroundImage = icon;
        itemLabel.text = label;
        itemWindow = window;
        // When button is clicked, open itemWindow in DesktopContents
        itemButton.clicked += () =>
        {
            var windowContainer = itemWindow.Instantiate();
            desktopContents.Add(windowContainer);
        };
    }

    public DesktopItemElement() {}
}