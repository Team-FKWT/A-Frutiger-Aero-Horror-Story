using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// TEMPORARY level ending. Put this on an object with a Box Collider.
/// When the player walks into the box, it shows "you completed this level"
/// Wait for a few seconds and then loads the intro screen.

[RequireComponent(typeof(BoxCollider))]
public class TEMPending_script : MonoBehaviour
{
    private string message = "you completed this level";
    private float secondsBeforeLoading = 3f;
    private string introScene = "IntroScreen";

    private bool finished;
    private GUIStyle textStyle;

    private void Reset()
    {
        // The box has to be a trigger so the player can walk into it
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (finished || other.GetComponentInParent<movementController>() == null) return;
        finished = true;
        StartCoroutine(LoadIntroAfterDelay());
    }

    private IEnumerator LoadIntroAfterDelay()
    {
        yield return new WaitForSecondsRealtime(secondsBeforeLoading);

        Time.timeScale = 1f;

        if (ProgressionState.Instance != null)
            Destroy(ProgressionState.Instance.gameObject);

        SceneManager.LoadScene(introScene);
    }

    private void OnGUI()
    {
        if (!finished) return;
        if (textStyle == null)
        {
            textStyle = new GUIStyle(GUI.skin.label) { fontSize = 40, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            textStyle.normal.textColor = Color.white;
        }
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), message, textStyle);
    }
}
