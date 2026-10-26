using UnityEngine;

public enum KeycardColor
{
    Red,
    Green,
    Blue
}

public class KeycardPuzzleState : MonoBehaviour
{
    public bool HasRed { get; private set; }
    public bool HasGreen { get; private set; }
    public bool HasBlue { get; private set; }

    public void CollectCard(KeycardColor color)
    {
        switch (color)
        {
            case KeycardColor.Red:
                HasRed = true;
                break;

            case KeycardColor.Green:
                HasGreen = true;
                break;

            case KeycardColor.Blue:
                HasBlue = true;
                break;
        }

        Debug.Log("Collected " + color + " keycard.");
    }

    public bool HasCard(KeycardColor color)
    {
        return color switch
        {
            KeycardColor.Red => HasRed,
            KeycardColor.Green => HasGreen,
            KeycardColor.Blue => HasBlue,
            _ => false
        };
    }
}