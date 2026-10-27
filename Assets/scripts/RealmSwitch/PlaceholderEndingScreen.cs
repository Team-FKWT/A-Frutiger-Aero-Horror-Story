using UnityEngine;

/// TEMPORARY stand-in for the real ending screens (built by Xander Puchalski and
/// Andrew How). When the ending starts, it pauses the game, frees the mouse, and
/// shows the two choices. Picking one calls ProgressionState.ChooseEnding and
/// shows that ending's text. Remove it once the real screens listen to the same events.
 
public class PlaceholderEndingScreen : MonoBehaviour
{
    [TextArea(3, 6)]
    [SerializeField] private string exposeTheTruthText =
        "You upload the corporate evidence. The company's stock crashes, its experiments are revealed to the public, and you face the dystopian world outside.";
    [TextArea(3, 6)]
    [SerializeField] private string remainIgnorantText =
        "You keep quiet. They catch you snooping and pay you to stay silent... and later make sure you stay that way.";

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

        GUI.Label(new Rect(x, y - 50f, w, 30f), "PLACEHOLDER ENDING SCREEN", bodyStyle);

        if (stage == Stage.Choosing)
        {
            GUI.Label(new Rect(x, y, w, 50f), "What will you do?", titleStyle);
            float bw = (w - 20f) / 2f;
            if (GUI.Button(new Rect(x, y + 90f, bw, 60f), "Expose the Truth", buttonStyle))
                state.ChooseEnding(EndingChoice.ExposeTheTruth);
            if (GUI.Button(new Rect(x + bw + 20f, y + 90f, bw, 60f), "Remain Ignorant", buttonStyle))
                state.ChooseEnding(EndingChoice.RemainIgnorant);
        }
        else
        {
            bool expose = chosen == EndingChoice.ExposeTheTruth;
            GUI.Label(new Rect(x, y, w, 50f), expose ? "Expose the Truth" : "Remain Ignorant", titleStyle);
            GUI.Label(new Rect(x, y + 70f, w, 160f), expose ? exposeTheTruthText : remainIgnorantText, bodyStyle);
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
