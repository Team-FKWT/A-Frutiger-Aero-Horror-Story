using UnityEngine;

/// TEMPORARY stand-in for the real ending screens, laid out from Xander Puchalski's
/// wireframe. When the ending starts, it pauses the game, frees the mouse, and
/// shows the two choices: Escape and Comply. Picking one calls
/// ProgressionState.ChooseEnding and shows that ending's text.
/// The ending text is a stand-in until Andrew How and Lily Gomes write the final versions.
/// Note: the text fields were renamed from exposeTheTruthText / remainIgnorantText so
/// scenes pick up the new wording instead of keeping the old saved text.

public class PlaceholderEndingScreen : MonoBehaviour
{
    [TextArea(3, 6)]
    [SerializeField]
    private string escapeText =
        "You shut down the data center and walk out through the emergency exit with the evidence. You know the truth about Hypnosom now, and Müllerin knows you know.";
    [TextArea(3, 6)]
    [SerializeField]
    private string complyText =
        "You disable the exit lock and wait. They said you weren't in trouble. They just wanted to talk.";

    private enum Stage { Hidden, Choosing, Showing }

    private ProgressionState state;
    private Stage stage = Stage.Hidden;
    private EndingChoice chosen;
    private GUIStyle titleStyle, bodyStyle, buttonStyle, boxStyle;

    private void Start()
    {
        state = ProgressionState.Instance;
        if (state == null)
        {
            Debug.LogError("PlaceholderEndingScreen needs a ProgressionState in the scene.");
            enabled = false;
            return;
        }
        state.EndingStarted += OnEndingStarted;
        state.EndingChosen += OnEndingChosen;
    }

    private void OnDestroy()
    {
        if (state == null) return;
        state.EndingStarted -= OnEndingStarted;
        state.EndingChosen -= OnEndingChosen;
    }

    private void OnEndingStarted()
    {
        stage = Stage.Choosing;
        Time.timeScale = 0f;                    // freeze the game behind the screen
        Cursor.lockState = CursorLockMode.None; // let the player click the buttons
        Cursor.visible = true;
    }

    private void OnEndingChosen(EndingChoice choice)
    {
        chosen = choice;
        stage = Stage.Showing;
    }

    private void OnGUI()
    {
        if (stage == Stage.Hidden) return;
        if (titleStyle == null) BuildStyles();
        GUI.depth = -10; // draw on top of other on-screen debug text

        // Keep the cursor free even if a player script tries to re-lock it.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none, boxStyle);
        float w = Mathf.Min(700f, Screen.width - 40f);
        float x = (Screen.width - w) / 2f;
        float y = Screen.height * 0.25f;

        if (stage == Stage.Choosing)
        {
            GUI.Label(new Rect(x, y, w, 50f), "What will you do?", titleStyle);
            float bw = (w - 20f) / 2f;
            // EndingChoice keeps its original internal names; only the on-screen words changed.
            if (GUI.Button(new Rect(x, y + 90f, bw, 60f), "Escape", buttonStyle))
                state.ChooseEnding(EndingChoice.ExposeTheTruth);
            if (GUI.Button(new Rect(x + bw + 20f, y + 90f, bw, 60f), "Comply", buttonStyle))
                state.ChooseEnding(EndingChoice.RemainIgnorant);
        }
        else
        {
            bool escape = chosen == EndingChoice.ExposeTheTruth;
            GUI.Label(new Rect(x, y, w, 50f), escape ? "Escape" : "Comply", titleStyle);
            GUI.Label(new Rect(x, y + 70f, w, 160f), escape ? escapeText : complyText, bodyStyle);
            GUI.Label(new Rect(x, y + 240f, w, 30f), "THE END  (press M to return to the menu)", bodyStyle);
        }
    }

    private void BuildStyles()
    {
        Texture2D dark = new Texture2D(1, 1);
        dark.SetPixel(0, 0, new Color(0.02f, 0.03f, 0.05f, 0.92f));
        dark.Apply();
        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = dark;

        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 40, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        titleStyle.normal.textColor = Color.white;
        bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.UpperCenter, wordWrap = true };
        bodyStyle.normal.textColor = new Color(0.85f, 0.9f, 0.95f);
        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 22, fontStyle = FontStyle.Bold };
    }
}