using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class DesktopWindowElement: VisualElement
{
    // Contents of window to display.
    [SerializeField] public VisualTreeAsset windowContents;
    
    // Components of the window.
    private Label windowTitle => this.Q<Label>("WindowTitle");
    private Button closeButton => this.Q<Button>("CloseButton");
    private VisualElement contentArea => this.Q<VisualElement>("WindowContents");

    public void Init(string title, VisualTreeAsset contents, Length width, Length height, Length xOffset, Length yOffset, bool absolute = false)
    {
        windowTitle.text = title;
        // Add provided contents into window
        windowContents = contents;
        var contentContainer = contents.Instantiate();
        contentArea.Add(contentContainer);
        contentContainer.style.width = Length.Percent(100);
        contentContainer.style.height = Length.Percent(100);
        style.width = width;
        style.height = height;
        style.left = xOffset;
        style.top = yOffset;
        style.position = (absolute ? Position.Absolute : Position.Relative);
        // Close button functionality
        closeButton.clicked += RemoveFromHierarchy;
    }

    public void Init(string title, VisualTreeAsset contents)
    {
        windowTitle.text = title;
        // Add provided contents into window
        windowContents = contents;
        var contentContainer = contents.Instantiate();
        contentArea.Add(contentContainer);
        contentContainer.style.width = Length.Percent(100);
        contentContainer.style.height = Length.Percent(100);
        // Close button functionality
        closeButton.clicked += RemoveFromHierarchy;
    }
}