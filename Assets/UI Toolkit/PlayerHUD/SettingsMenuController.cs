using UnityEngine;
using UnityEngine.UIElements;

public class SettingsMenuController : MonoBehaviour
{
    private Slider fovSlider;
    private Slider sensitivitySlider;
    private Slider volumeSlider;
    private Button applyButton;
    
    void Awake()
    {
        GetComponent<PanelRenderer>().RegisterUIReloadCallback(OnUIReload);
    }

    void OnDestroy()
    {
        GetComponent<PanelRenderer>().UnregisterUIReloadCallback(OnUIReload);
    }

    private void OnUIReload(PanelRenderer panelRenderer, VisualElement root, int version)
    {
        fovSlider = root.Q<Slider>("FOVSlider");
        sensitivitySlider = root.Q<Slider>("SensitivitySlider");
        volumeSlider = root.Q<Slider>("VolumeSlider");
        applyButton = root.Q<Button>("ApplyButton");
        
        
    }
}
