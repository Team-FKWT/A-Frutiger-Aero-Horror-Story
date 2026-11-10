using UnityEngine;
using UnityEngine.UI;
// 1. We must include the new Input System namespace at the top
using UnityEngine.InputSystem; 

public class StealthMeter : MonoBehaviour
{
    [Header("UI Reference")]
    public Slider stealthSlider;

    [Header("Stealth Integer Levels")]
    public int maxSuspicion = 100; 
    public int currentSuspicion = 0;

    [Header("Detection Settings")]
    public int detectionRate = 35;
    public int decayRate = 20;

    [Header("Testing Toggle")]
    public bool isPlayerVisible = false;

    void Start()
    {
        if (stealthSlider != null)
        {
            stealthSlider.wholeNumbers = true; 
            stealthSlider.minValue = 0;
            stealthSlider.maxValue = maxSuspicion;
            stealthSlider.value = currentSuspicion;
            stealthSlider.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // 2. This is the New Input System syntax to read the Spacebar key smoothly
        if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed)
        {
            isPlayerVisible = true;
        }
        else
        {
            isPlayerVisible = false;
        }

        // Calculate changes based on visibility
        if (isPlayerVisible)
        {
            float incrementalIncrease = detectionRate * Time.deltaTime;
            currentSuspicion += Mathf.RoundToInt(incrementalIncrease > 0 ? Mathf.Max(1, incrementalIncrease) : 0);
        }
        else
        {
            float incrementalDecay = decayRate * Time.deltaTime;
            currentSuspicion -= Mathf.RoundToInt(incrementalDecay > 0 ? Mathf.Max(1, incrementalDecay) : 0);
        }

        currentSuspicion = Mathf.Clamp(currentSuspicion, 0, maxSuspicion);

        if (stealthSlider != null)
        {
            stealthSlider.value = currentSuspicion;
            stealthSlider.gameObject.SetActive(currentSuspicion > 0);
        }
    }
}
