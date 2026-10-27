using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// Sends the player back to the intro menu.
/// On the player: press the menu key from anywhere in the level.
/// On a cube with a trigger collider: walking into it returns to the menu.
public class ReturnToIntro : MonoBehaviour
{
    [SerializeField] private string introScene = "IntroScreen";
    [SerializeField] private bool listenForKey = true;
    [SerializeField] private Key menuKey = Key.M;

    private bool leaving;

    void Update()
    {
        if (listenForKey && Keyboard.current != null && Keyboard.current[menuKey].wasPressedThisFrame)
            GoToIntro();
    }

    void OnTriggerEnter(Collider other)
    {
        // Check for the player's movement script instead of a tag, since not every scene tags the player
        if (other.GetComponentInParent<movementController>() != null)
            GoToIntro();
    }

    public void GoToIntro()
    {
        if (leaving) return;
        leaving = true;
        Time.timeScale = 1f; // unfreeze in case the ending screen paused the game

        // Clear the carried-over progress so the next run starts fresh
        if (ProgressionState.Instance != null)
            Destroy(ProgressionState.Instance.gameObject);

        SceneManager.LoadScene(introScene);
    }
}