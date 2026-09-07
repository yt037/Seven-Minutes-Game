// Keys.cs
// Single place that touches the keyboard and mouse directly. Everything that is
// not in Player.inputactions (Tab, Escape, 1-4, F-keys, a bare click) goes
// through here so no script uses the legacy Input class.
using UnityEngine.InputSystem;

public static class Keys
{
    public static bool TabPressed => Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame;
    public static bool EscapePressed => Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
    public static bool LeftClickPressed => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

    // 1 to 4, used for dialogue choices.
    public static bool DigitPressed(int digit)
    {
        var kb = Keyboard.current;
        if (kb == null) return false;

        switch (digit)
        {
            case 1: return kb.digit1Key.wasPressedThisFrame;
            case 2: return kb.digit2Key.wasPressedThisFrame;
            case 3: return kb.digit3Key.wasPressedThisFrame;
            case 4: return kb.digit4Key.wasPressedThisFrame;
            default: return false;
        }
    }

    // F5 to F9, editor and development builds only, used by GameManager debug hotkeys.
    public static bool FunctionPressed(int n)
    {
        var kb = Keyboard.current;
        if (kb == null) return false;

        switch (n)
        {
            case 5: return kb.f5Key.wasPressedThisFrame;
            case 6: return kb.f6Key.wasPressedThisFrame;
            case 7: return kb.f7Key.wasPressedThisFrame;
            case 8: return kb.f8Key.wasPressedThisFrame;
            case 9: return kb.f9Key.wasPressedThisFrame;
            default: return false;
        }
    }
}
