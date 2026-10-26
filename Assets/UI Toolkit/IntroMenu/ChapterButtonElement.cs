using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ChapterButtonElement: VisualElement
{
    // References to ChapterButton elements
    private VisualElement ChapterIcon => this.Q("ChapterIcon");
    private Label ChapterLabel => this.Q<Label>("ChapterLabel");
    private Button ChapterButton => this.Q<Button>("ChapterButton");
    private string ChapterScene;

    // Sets button properties when instantiated.
    public void Init(Texture2D icon, int chapter, string scene, GameObject loadingScreenPanel)
    {
        ChapterIcon.style.backgroundImage = icon;
        ChapterLabel.text = "Chapter " + chapter;
        ChapterScene = scene;
        // On click, load scene specified in controller script.
        ChapterButton.clicked += () =>
        {
            // Tell controller to load the scene
            loadingScreenPanel.GetComponent<LoadingScreenController>().sceneName = scene;
            loadingScreenPanel.SetActive(true);
        };
    }

    public ChapterButtonElement() {}
}