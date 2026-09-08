// Door.cs
// A door the player opens by pressing E. Optionally needs a run flag. Once
// open it stays open for the run. Office door and vault door.
using UnityEngine;

public class Door : MonoBehaviour, Interaction
{
    [SerializeField] private Animator animator;
    [SerializeField] private string openTriggerName = "Open";
    [SerializeField] private string displayName = "Door";
    [SerializeField] private string requiresFlag = "";
    [SerializeField] private string lockedLine = "Locked.";

    public bool IsOpen { get; private set; }

    public string Prompt => IsOpen ? null : displayName;

    public void Interact(Player player)
    {
        if (IsOpen) return;

        GameManager gm = GameManager.Instance;
        bool locked = !string.IsNullOrEmpty(requiresFlag) && (gm == null || !gm.HasFlag(requiresFlag));

        if (locked)
        {
            if (DialogueRunner.Instance != null) DialogueRunner.Instance.ShowLine(lockedLine);
            return;
        }

        Open();
    }

    // Also callable from UnityEvents (Vault.Open uses it).
    public void Open()
    {
        if (IsOpen) return;
        IsOpen = true;

        if (animator != null) animator.SetTrigger(openTriggerName);
        if (AudioManager.Instance != null) AudioManager.Instance.Play(GameIds.SfxDoor);
    }
}
