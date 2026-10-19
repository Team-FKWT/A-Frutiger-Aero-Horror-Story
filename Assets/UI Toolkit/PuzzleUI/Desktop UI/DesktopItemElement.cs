using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class DesktopItemElement: VisualElement
{
    // References to item attributes
    private VisualElement itemIcon => this.Q<VisualElement>("ItemIcon");
    private Label itemLabel => this.Q<Label>("ItemLabel");
    private Button itemButton => this.Q<Button>("DesktopItem");
    
    // Contents of windows to open when item clicked.
    private VisualTreeAsset itemContents;
    
    // Reference to parent tree to add window to.
    private VisualElement desktopContents;

    public void Init(Texture2D icon, string label, VisualTreeAsset contents, VisualElement contentArea, VisualTreeAsset windowTemplate)
    {
        desktopContents = contentArea;
        itemContents = contents;
        // Set visible elements from parameters
        itemIcon.style.backgroundImage = icon;
        itemLabel.text = label;
        // When button is clicked, open itemWindow in DesktopContents
        itemButton.clicked += () =>
        {
            // Create new DesktopWindowElement
            var windowContainer = windowTemplate.Instantiate();
            var desktopWindow = windowContainer.Q<DesktopWindowElement>();
            contentArea.Add(desktopWindow);
            // Instantiate window with provided contents
            desktopWindow.Init(label, contents);
        };
    }

    public DesktopItemElement() {}
}