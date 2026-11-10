using UnityEngine;

/// Lets keyboard shortcuts (menu, realm switch, debug keys, closing a screen)
/// stand down while the player is typing in an on-screen text box, so typing
/// "Meow" into a password field doesn't send the player to the menu.
/// A text box calls SetTyping(true) when it gets focus and SetTyping(false)
/// when it loses it. Any script that reads a key should check IsTyping first.
public static class TextInputGuard
{
    /// True while an on-screen text box has keyboard focus.
    public static bool IsTyping { get; private set; }

    /// Called by text boxes when they gain or lose focus.
    public static void SetTyping(bool typing) => IsTyping = typing;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay() => IsTyping = false;
}
