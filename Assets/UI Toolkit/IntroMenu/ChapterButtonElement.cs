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
    public void Init(Texture2D icon, int chapter, string scene)
    {
        ChapterIcon.style.backgroundImage = icon;
        ChapterLabel.text = "Chapter " + chapter;
        ChapterScene = scene;
        // On click, load scene specified in controller script.
        ChapterButton.clicked += () =>
        {
            // Check that the specified scene exists
            if (SceneManager.GetSceneByName(ChapterScene).IsValid())
                SceneManager.LoadScene(ChapterScene);
            else
                Debug.LogError("Chapter " + chapter + " button tried to load invalid scene \"" + ChapterScene + "\"");
        };
    }

    public ChapterButtonElement() {}
}