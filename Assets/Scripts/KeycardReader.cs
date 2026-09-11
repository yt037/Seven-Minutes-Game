// KeycardReader.cs
// Wall unit beside a locked AutoDoor. Needs the right card flag. An optional
// guard (an NPC in blocking state) can refuse the player unless a pass flag is
// held. On success the door unlocks for the run and onUnlocked fires.
using UnityEngine;
using UnityEngine.Events;

public class KeycardReader : MonoBehaviour, Interaction
{
    [SerializeField] private string requiredFlag = "";
    [SerializeField] private AutoDoor door;

    [Header("Guard (optional)")]
    [SerializeField] private NPC guard;
    [Tooltip("If the guard is blocking and this flag is not held, the guard refuses.")]
    [SerializeField] private string guardPassFlag = "";
    [SerializeField] private string guardBlockedLine = "";
    [SerializeField] private string guardPassLine = "";

    [Header("Lines")]
    [SerializeField] private string usePrompt = "Use keycard";
    [SerializeField] private string lockedPrompt = "Needs a keycard";
    [SerializeField] private string lockedLine = "The reader blinks red.";

    public UnityEvent onUnlocked;

    public bool Used { get; private set; }

    public string Prompt
    {
        get
        {
            if (Used) return null;
            return HasCard ? usePrompt : lockedPrompt;
        }
    }

    private bool HasCard => GameManager.Instance != null && GameManager.Instance.HasFlag(requiredFlag);

    public void Interact(Player player)
    {
        if (Used) return;

        if (!HasCard)
        {
            if (DialogueRunner.Instance != null) DialogueRunner.Instance.ShowLine(lockedLine);
            return;
        }

        bool guarded = guard != null && guard.Blocking && !string.IsNullOrEmpty(guardPassFlag);
        if (guarded && !GameManager.Instance.HasFlag(guardPassFlag))
        {
            guard.Say(guardBlockedLine);
            return;
        }

        Used = true;
        if (door != null) door.Unlock();
        if (guard != null && guard.Blocking && !string.IsNullOrEmpty(guardPassLine)) guard.Say(guardPassLine);
        if (AudioManager.Instance != null) AudioManager.Instance.Play(GameIds.SfxDoor);AudioManager.Instance.Play(GameIds.SfxCardReader);
        onUnlocked?.Invoke();
    }
}
