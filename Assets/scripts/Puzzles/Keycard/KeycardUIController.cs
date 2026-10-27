using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class KeycardUIController : MonoBehaviour
{
    [SerializeField] private KeycardPuzzleState puzzleState;

    [Header("Progression")]
    [SerializeField] private string puzzleId = "S1_Keycards";

    [SerializeField] public PuzzleActor puzzleActor;
    [SerializeField] public int cardsRequired = 3;

    private Button redCard;
    private Button greenCard;
    private Button blueCard;
    private Button authorizeButton;

    private Label statusText;

    private VisualElement slot1;
    private VisualElement slot2;
    private VisualElement slot3;

    private readonly List<KeycardColor> insertedCards = new();

    private void Awake()
    {
        GetComponent<PanelRenderer>()
            .RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDestroy()
    {
        GetComponent<PanelRenderer>()
            .UnregisterUIReloadCallback(OnUIReload);
    }

    private void OnUIReload(
    PanelRenderer panelRenderer,
    VisualElement root,
    int version) //supertemp
    {
    Debug.Log("Keycard UI reloaded");

    redCard = root.Q<Button>("RedCard");
    greenCard = root.Q<Button>("GreenCard");
    blueCard = root.Q<Button>("BlueCard");

    authorizeButton = root.Q<Button>("AuthorizeButton");
    statusText = root.Q<Label>("StatusText");

    slot1 = root.Q<VisualElement>("Slot1");
    slot2 = root.Q<VisualElement>("Slot2");
    slot3 = root.Q<VisualElement>("Slot3");

    if (redCard == null)
        Debug.LogError("Missing: RedCard");

    if (greenCard == null)
        Debug.LogError("Missing: GreenCard");

    if (blueCard == null)
        Debug.LogError("Missing: BlueCard");

    if (authorizeButton == null)
        Debug.LogError("Missing: AuthorizeButton");

    if (statusText == null)
        Debug.LogError("Missing: StatusText");

    if (slot1 == null)
        Debug.LogError("Missing: Slot1");

    if (slot2 == null)
        Debug.LogError("Missing: Slot2");

    if (slot3 == null)
        Debug.LogError("Missing: Slot3");

    if (redCard == null ||
        greenCard == null ||
        blueCard == null ||
        authorizeButton == null ||
        statusText == null ||
        slot1 == null ||
        slot2 == null ||
        slot3 == null)
    {
        Debug.LogError("Keycard UI setup incomplete — check UXML names/source asset.");
        return;
    }

    redCard.clicked += () => InsertCard(KeycardColor.Red);
    greenCard.clicked += () => InsertCard(KeycardColor.Green);
    blueCard.clicked += () => InsertCard(KeycardColor.Blue);

    authorizeButton.clicked += CheckAccess;

    RefreshCards();
    RefreshSlots();

    Debug.Log("Keycard UI setup successful");
    }

    public void RefreshUI()
    {
        if (puzzleState == null)
        {
            Debug.LogError("KeycardUIController has no Puzzle State assigned.");
            return;
        }

        if (redCard == null ||
            greenCard == null ||
            blueCard == null ||
            authorizeButton == null ||
            statusText == null ||
            slot1 == null ||
            slot2 == null ||
            slot3 == null)
        {
            Debug.LogWarning("Keycard UI hasn't initialized yet.");
            return;
        }

        RefreshCards();
        RefreshSlots();
    }
    private void RefreshCards()
    {
        redCard.SetEnabled(puzzleState.HasRed);
        greenCard.SetEnabled(puzzleState.HasGreen);
        blueCard.SetEnabled(puzzleState.HasBlue);

        redCard.style.opacity = puzzleState.HasRed ? 1f : 0.2f;
        greenCard.style.opacity = puzzleState.HasGreen ? 1f : 0.2f;
        blueCard.style.opacity = puzzleState.HasBlue ? 1f : 0.2f;
    }

    private void InsertCard(KeycardColor color)
    {
        if (!puzzleState.HasCard(color))
        {
            statusText.text = "CARD NOT FOUND";
            return;
        }

        if (insertedCards.Contains(color))
            return;

        if (insertedCards.Count >= 3)
            return;

        insertedCards.Add(color);

        statusText.text = "CARD ACCEPTED";

        RefreshSlots();
    }

    private void RefreshSlots()
    {
        SetSlotColor(slot1, 0);
        SetSlotColor(slot2, 1);
        SetSlotColor(slot3, 2);
    }

    private void SetSlotColor(VisualElement slot, int index)
    {
        if (index >= insertedCards.Count)
        {
            slot.style.backgroundColor = new Color(0.04f, 0.04f, 0.04f);
            return;
        }

        slot.style.backgroundColor =
            insertedCards[index] switch
            {
                KeycardColor.Red => Color.red,
                KeycardColor.Green => Color.green,
                KeycardColor.Blue => Color.blue,
                _ => Color.black
            };
    }

    private void CheckAccess()
    {
        bool correct = (insertedCards.Count == cardsRequired);
        if (cardsRequired >= 1)
            correct = correct && insertedCards[0] == KeycardColor.Red;
        if (cardsRequired >= 2)
            correct = correct && insertedCards[1] == KeycardColor.Green;
        if (cardsRequired >= 3)
            correct = correct && insertedCards[2] == KeycardColor.Blue;

        if (correct)
        {
            statusText.text = "ACCESS GRANTED";

            if (ProgressionState.Instance != null)
            {
                ProgressionState.Instance.MarkPuzzleSolved(puzzleId);
            }
            
            // If PuzzleActor attached, call OnSolve
            if (puzzleActor != null)
            {
                puzzleActor.OnSolve();
            }

            authorizeButton.SetEnabled(false);
        }
        else
        {
            statusText.text = "ACCESS DENIED";

            insertedCards.Clear();
            RefreshSlots();
        }
    }
}