// TalkingNPC.cs
// Any NPC the player can talk to. Points at a JSON file; the file's startRules
// choose the opening line. Tellers also carry the pre-heist skip rule.
using UnityEngine;

public class TalkingNPC : MonoBehaviour, Interaction
{
    [SerializeField] private string dialogueFile = "";
    [Tooltip("Used if there is no NameTag on this object.")]
    [SerializeField] private string displayName = "";
    [Tooltip("Not talkable until this run flag is held. Robbers use heist_started.")]
    [SerializeField] private string requiredFlag = "";

    [Header("Tellers only")]
    [SerializeField] private bool isTeller;
    [Tooltip("First run: before this many seconds the teller nudges the player to talk to the queue instead.")]
    [SerializeField] private float tellerNudgeBefore = 50f;

    private NameTag nameTag;
    private bool pendingSkip;

    private void Awake() => nameTag = GetComponent<NameTag>();

    private bool Available
    {
        get
        {
            if (!enabled) return false;
            if (string.IsNullOrEmpty(requiredFlag)) return true;
            return GameManager.Instance != null && GameManager.Instance.HasFlag(requiredFlag);
        }
    }

    public string Prompt
    {
        get
        {
            if (!Available) return null;
            if (nameTag != null && !string.IsNullOrEmpty(nameTag.DisplayName)) return nameTag.DisplayName;
            return string.IsNullOrEmpty(displayName) ? name : displayName;
        }
    }

    public void Interact(Player player)
    {
        if (!Available) return;

        DialogueRunner runner = DialogueRunner.Instance;
        if (runner == null || string.IsNullOrEmpty(dialogueFile))
        {
            Debug.LogWarning($"{name}: no DialogueRunner or no dialogue file.");
            return;
        }

        if (isTeller && HandleTeller(runner)) return;

        runner.StartDialogue(dialogueFile, transform);
    }

    // Returns true if the teller logic consumed the interaction.
    private bool HandleTeller(DialogueRunner runner)
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.Timer == null || !gm.Timer.Running || gm.HasFlag(GameIds.HeistStarted)) return false;

        if (gm.State.IsFirstRun && gm.Timer.Elapsed < tellerNudgeBefore && !gm.HasFlag(GameIds.TellerNudged))
        {
            gm.SetFlag(GameIds.TellerNudged);
            runner.Play(GameIds.DlgTellerNudge);
            return true;
        }

        if (!pendingSkip)
        {
            pendingSkip = true;
            runner.Finished += OnConversationFinished;
        }
        return false;
    }

    private void OnConversationFinished()
    {
        DialogueRunner runner = DialogueRunner.Instance;
        if (runner != null) runner.Finished -= OnConversationFinished;
        pendingSkip = false;

        GameManager gm = GameManager.Instance;
        if (gm == null || gm.Timer == null) return;
        if (gm.HasFlag(GameIds.HeistStarted) || !gm.HasFlag(GameIds.SkipAhead)) return;

        gm.Timer.JumpToHeist();
    }
}
