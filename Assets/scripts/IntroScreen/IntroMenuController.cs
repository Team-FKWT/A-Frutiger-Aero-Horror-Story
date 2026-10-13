using UnityEngine;
using UnityEngine.UIElements;

public class IntroMenuController: MonoBehaviour
{
    private VisualElement centredDiv;
    private int clicked;

    public VisualElement chapterButton1;
    public VisualElement chapterButton2;
    public VisualElement chapterButton3;

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
        centredDiv = root.Q<VisualElement>("CentredDiv");
        // Get currently unlocked chapters from global state script
        switch (ProgressionState.Instance.CurrentSegment)
        {
        }
        chapterButton1 = root.Q<Button>("ChapterButton1");
        
        chapterButton1.RegisterCallback<ClickEvent>(PrintClickMsg);
    }

    private void PrintClickMsg(ClickEvent e)
    {
        clicked++;
        Debug.Log("clicked: " + clicked + " times");
    }
    

}