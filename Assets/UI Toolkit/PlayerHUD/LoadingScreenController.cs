using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LoadingScreenController : MonoBehaviour
{
    [SerializeField] public string sceneName;
    [SerializeField] public float FadeDuration = 3f;
    
    private Label loadingText;
    private VisualElement background;
    
    void Awake()
    {
        // Enable loading screen panel renderer
        GetComponent<PanelRenderer>().RegisterUIReloadCallback(OnUIReload);
    }

    void OnDestroy()
    {
        GetComponent<PanelRenderer>().UnregisterUIReloadCallback(OnUIReload);
    }

    void OnUIReload(PanelRenderer panelRenderer, VisualElement root, int version)
    {
        // Get reference to loading text
        loadingText = root.Q<Label>("LoadingText");
        background = root.Q<VisualElement>("Background");
        // Start coroutine to load next scene
        StartCoroutine(LoadNewScene(sceneName));
    }

    void Update()
    {
        // Fade in background
        if (background != null)
        {
            float alpha = 0;
            float fadeSpeed = Mathf.Abs(alpha - 255) / FadeDuration;
            while (!Mathf.Approximately(alpha, 255))
            {
                alpha = Mathf.MoveTowards(alpha, 255, fadeSpeed * Time.deltaTime);
                background.style.backgroundColor = new Color(0, 0, 0, alpha);
            }
        }

        // Pulse text transparency while loading
        if (loadingText != null)
            loadingText.style.color = new Color(255, 255,255 , Mathf.PingPong(Time.time, 1));
    }
    
    // Coroutine for loading streamed scene
    IEnumerator LoadNewScene(string sceneName)
    {
        // Artificial delay for demonstrating loading screen
        yield return new WaitForSeconds(3);
        // Check that the specified scene exists
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            // Wait for async load to finish
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
    }
}