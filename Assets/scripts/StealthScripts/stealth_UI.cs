using UnityEngine;
using UnityEngine.UI;

public class stealth_UI : MonoBehaviour
{
    public GameObject player;
    public Image detectionBar;
    public Image alertImage;
    public float alertThreshold = 80f;

    private movementController playerController;
    private float fullBarHeight;

    void Start()
    {
        playerController = player.GetComponent<movementController>();

        // the bar's height in the editor is treated as 100%, set its pivot to the bottom so it grows upwards
        fullBarHeight = detectionBar.rectTransform.sizeDelta.y;
        alertImage.gameObject.SetActive(false);
    }

    void Update()
    {
        UpdateDetectionBar(detectionBar, alertImage, playerController.detectionLevel);
    }

    // scales the bar from 0% to 100% height based on detection level (0-100), and shows the alert image once it hits the threshold
    public void UpdateDetectionBar(Image bar, Image alert, float detectionLevel)
    {
        float percent = Mathf.Clamp01(detectionLevel / 100f);

        Vector2 size = bar.rectTransform.sizeDelta;
        size.y = fullBarHeight * percent;
        bar.rectTransform.sizeDelta = size;

        alert.gameObject.SetActive(detectionLevel >= alertThreshold);
    }
}
