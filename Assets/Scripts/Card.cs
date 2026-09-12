// Card.cs
// The generic named interactable. Pickups, the alarm button, the note, the
// carpet, and Start and Quit in the house all use it. It grants a flag, can
// award a clue, can play a line, fires a UnityEvent, and names itself in the
// crosshair prompt.
using UnityEngine;
using UnityEngine.Events;

public class Card : MonoBehaviour, Interaction
{
    [SerializeField] private string displayName = "Item";
    [SerializeField] private string grantsFlag = "";
    [SerializeField] private string awardsClue = "";
    [Tooltip("Dialogue reference played on use, e.g. player_self#carpet_key")]
    [SerializeField] private string dialogueRef = "";
    [Tooltip("Shown on later interactions when once is set and the object stays.")]
    [SerializeField] private string usedLine = "";
    [Tooltip("The prompt stays hidden until this clue is in the log. E still works.")]
    [SerializeField] private string promptRequiresClue = "";
    [SerializeField] private bool destroyOnUse = true;
    [SerializeField] private bool once = true;
    [SerializeField] private string soundId = "";
    public UnityEvent onUsed;

    private bool used;

    public string Prompt
    {
        get
        {
            if (!string.IsNullOrEmpty(promptRequiresClue))
            {
                GameManager gm = GameManager.Instance;
                if (gm == null || gm.Clues == null || !gm.Clues.Has(promptRequiresClue)) return null;
            }

            if (used && once && string.IsNullOrEmpty(usedLine)) return null;
            return displayName;
        }
    }

    public void Interact(Player player)
    {
        if (used && once)
        {
            if (!string.IsNullOrEmpty(usedLine) && DialogueRunner.Instance != null) DialogueRunner.Instance.ShowLine(usedLine);
            return;
        }

        used = true;
        if (!string.IsNullOrEmpty(soundId) && AudioManager.Instance != null)
        AudioManager.Instance.Play(soundId);

        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            if (!string.IsNullOrEmpty(grantsFlag)) gm.SetFlag(grantsFlag);
            if (!string.IsNullOrEmpty(awardsClue)) gm.AddClue(awardsClue);
        }

        onUsed?.Invoke();

        if (!string.IsNullOrEmpty(dialogueRef) && DialogueRunner.Instance != null) DialogueRunner.Instance.Play(dialogueRef);

        if (destroyOnUse) Destroy(gameObject);
    }
}
