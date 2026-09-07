// DialogueRunner.cs
// Reads a dialogue JSON from StreamingAssets and walks its nodes. Applies
// flags and clues, filters choices, holds the suspicion counter. Every line
// the player reads goes through here, including one-liners (ShowLine), so the
// 2 second advance lock and the movement freeze behave the same everywhere.
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class DialogueRunner : MonoBehaviour
{
    public static DialogueRunner Instance { get; private set; }

    public DialogueManager ui;
    public string folder = "Dialogue";

    [Tooltip("Seconds a line must be on screen before E or a number key does anything.")]
    public float lineLockSeconds = 2f;

    public UnityEvent onStarted;
    public UnityEvent onFinished;
    public event Action Finished;

    public bool IsRunning { get; private set; }
    public Transform Speaker { get; private set; }
    public int Suspicion { get; private set; }

    private readonly Dictionary<string, DialogueFile> cache = new Dictionary<string, DialogueFile>();
    private readonly List<DialogueChoice> visible = new List<DialogueChoice>();

    private DialogueFile file;
    private DialogueNode node;
    private float shownAt;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (!IsRunning || visible.Count == 0 || PauseMenu.IsPaused) return;
        if (Time.unscaledTime - shownAt < lineLockSeconds) return;

        for (int i = 0; i < visible.Count && i < 4; i++)
        {
            if (Keys.DigitPressed(i + 1)) { Choose(i); return; }
        }
    }

    // ---------------------------------------------------------------- starting

    // reference is "file" or "file#node". Refused while something is running.
    public bool Play(string reference)
    {
        if (IsRunning) return false;
        return Begin(reference, null);
    }

    // Same, but interrupts whatever is running. Used by beats and deaths.
    public bool PlayForced(string reference)
    {
        if (IsRunning) End();
        return Begin(reference, null);
    }

    // Void wrappers so UnityEvents in the Inspector can call these. A persistent
    // listener needs a void method on a real component, and Play/PlayForced
    // return bool.
    public void PlayRef(string reference) => Play(reference);

    public void PlayForcedRef(string reference) => PlayForced(reference);

    // Used by TalkingNPC: file's startRules pick the opening node.
    public bool StartDialogue(string fileName, Transform speaker)
    {
        if (IsRunning) return false;
        return Begin(fileName, speaker);
    }

    // A single line with no speaker (locked doors, pickups that say something).
    public void ShowLine(string text)
    {
        if (IsRunning || string.IsNullOrEmpty(text)) return;

        var synthetic = new DialogueFile
        {
            startNode = "line",
            nodes = new[] { new DialogueNode { id = "line", text = text, next = "" } }
        };

        Run(synthetic, "line", null);
    }

    public void Interrupt()
    {
        if (IsRunning) End();
    }

    private bool Begin(string reference, Transform speaker)
    {
        if (string.IsNullOrEmpty(reference)) return false;

        string fileName = reference;
        string nodeId = null;
        int hash = reference.IndexOf('#');
        if (hash >= 0)
        {
            fileName = reference.Substring(0, hash);
            nodeId = reference.Substring(hash + 1);
        }

        DialogueFile loaded = Load(fileName);
        if (loaded == null || loaded.nodes == null || loaded.nodes.Length == 0)
        {
            Debug.LogWarning($"Dialogue file empty or missing: {fileName}");
            return false;
        }

        if (string.IsNullOrEmpty(nodeId)) nodeId = PickStart(loaded);
        return Run(loaded, nodeId, speaker);
    }

    private static string PickStart(DialogueFile f)
    {
        if (f.startRules != null)
            foreach (StartRule r in f.startRules)
                if (r != null && !string.IsNullOrEmpty(r.node) && Held(r.flag)) return r.node;

        return f.startNode;
    }

    private bool Run(DialogueFile f, string nodeId, Transform speaker)
    {
        file = f;
        Speaker = speaker;
        Suspicion = 0;
        IsRunning = true;

        if (ui != null) ui.OpenDialogue();
        onStarted?.Invoke();

        Goto(nodeId);
        return true;
    }

    // ---------------------------------------------------------------- stepping

    public void Advance()
    {
        if (!IsRunning || PauseMenu.IsPaused) return;
        if (Time.unscaledTime - shownAt < lineLockSeconds) return;
        if (visible.Count > 0) return;
        if (node == null) { End(); return; }

        Goto(node.next);
    }

    public void Choose(int index)
    {
        if (!IsRunning || index < 0 || index >= visible.Count) return;

        DialogueChoice choice = visible[index];
        visible.Clear();
        if (ui != null) ui.HideChoices();

        Suspicion += choice.suspicion;
        ApplyFlags(choice.setFlags, choice.setPersistentFlags, null);

        if (!string.IsNullOrEmpty(choice.ending))
        {
            GameManager gm = GameManager.Instance;
            if (choice.ending == GameIds.EndingFailure && gm != null)
            {
                gm.SetFlag(GameIds.FatalDialogue);
                gm.SetFlag(GameIds.ResolveNow);
            }
            else
            {
                Debug.LogWarning($"Choice 'ending' = {choice.ending} ignored. Use setFlags; only 'failure' is honoured.");
            }
            End();
            return;
        }

        Goto(choice.next);
    }

    private void Goto(string id)
    {
        if (string.IsNullOrEmpty(id)) { End(); return; }

        node = Find(id);
        if (node == null)
        {
            Debug.LogWarning($"Dialogue node not found: {id}");
            End();
            return;
        }

        Show(node);
    }

    private void Show(DialogueNode n)
    {
        shownAt = Time.unscaledTime;
        ApplyFlags(n.setFlags, n.setPersistentFlags, n.addClues);

        if (ui != null)
        {
            ui.SetSpeaker(n.speaker);
            ui.ChangeDialogueText(PickText(n));
        }

        visible.Clear();
        if (n.choices != null)
            foreach (DialogueChoice c in n.choices)
                if (Allowed(c)) visible.Add(c);

        int cap = ui != null ? ui.ChoiceSlots : 4;
        if (cap > 0 && visible.Count > cap)
        {
            Debug.LogWarning($"Node {n.id} has {visible.Count} visible choices, UI shows {cap}. Extra choices dropped.");
            visible.RemoveRange(cap, visible.Count - cap);
        }

        if (ui == null) return;
        if (visible.Count == 0) { ui.HideChoices(); return; }

        var labels = new string[visible.Count];
        for (int i = 0; i < visible.Count; i++) labels[i] = visible[i].text;
        ui.ShowChoices(labels);
    }

    private bool Allowed(DialogueChoice c)
    {
        if (c == null) return false;
        if (Suspicion < c.minSuspicion) return false;

        if (c.requiredFlags != null)
            foreach (string f in c.requiredFlags)
                if (!Held(f)) return false;

        if (c.blockedFlags != null)
            foreach (string f in c.blockedFlags)
                if (Held(f)) return false;

        return true;
    }

    // Run flags, persistent flags and clue ids all count as "held".
    public static bool Held(string flag)
    {
        if (string.IsNullOrEmpty(flag)) return true;

        GameManager gm = GameManager.Instance;
        if (gm == null) return false;

        if (gm.HasFlag(flag) || gm.KnowsPersistent(flag)) return true;
        return gm.Clues != null && gm.Clues.Has(flag);
    }

    private static void ApplyFlags(string[] loop, string[] persistent, string[] clues)
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        if (loop != null) foreach (string f in loop) gm.SetFlag(f);
        if (persistent != null) foreach (string f in persistent) gm.LearnPersistent(f);
        if (clues != null) foreach (string c in clues) gm.AddClue(c);
    }

    private static string PickText(DialogueNode n)
    {
        if (n.textVariants != null && n.textVariants.Length > 0)
            return n.textVariants[UnityEngine.Random.Range(0, n.textVariants.Length)];
        return n.text ?? "";
    }

    private DialogueNode Find(string id)
    {
        foreach (DialogueNode n in file.nodes)
            if (n != null && n.id == id) return n;
        return null;
    }

    private DialogueFile Load(string fileName)
    {
        if (cache.TryGetValue(fileName, out DialogueFile hit)) return hit;

        string name = fileName.EndsWith(".json") ? fileName.Substring(0, fileName.Length - 5) : fileName;
        string path = string.IsNullOrEmpty(folder) ? name : folder + "/" + name;

        try
        {
            TextAsset asset = Resources.Load<TextAsset>(path);
            if (asset == null)
            {
                Debug.LogWarning($"Dialogue file not found in Resources: {path}");
                return null;
            }

            DialogueFile parsed = JsonUtility.FromJson<DialogueFile>(asset.text);
            cache[fileName] = parsed;
            return parsed;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Dialogue load failed for {fileName}: {e.Message}");
            return null;
        }
    }

    private void End()
    {
        IsRunning = false;
        node = null;
        file = null;
        Speaker = null;
        visible.Clear();

        if (ui != null) ui.CloseDialogue();
        Finished?.Invoke();
        onFinished?.Invoke();
    }
}
