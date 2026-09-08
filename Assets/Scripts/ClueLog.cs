// ClueLog.cs
// On the GameManager object. Loads clue text from StreamingAssets/clues.json
// and manages the session clue list held in GameManager.State. Adding a clue
// that supersedes another removes the old one, so the log rewrites itself
// when the player learns better.
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class ClueLog : MonoBehaviour
{
    [SerializeField] private string clueFileName = "clues.json";
    public UnityEvent onChanged;
    public event Action<Clue> Added;

    private readonly Dictionary<string, Clue> library = new Dictionary<string, Clue>();
    private readonly List<Clue> catalog = new List<Clue>();
    private bool loaded;

    public IReadOnlyList<Clue> Catalog { get { LoadLibrary(); return catalog; } }

    private List<string> Ids => GameManager.Instance != null ? GameManager.Instance.State.clues : null;

    private void Awake() => LoadLibrary();

    public bool Has(string id)
    {
        List<string> ids = Ids;
        return ids != null && !string.IsNullOrEmpty(id) && ids.Contains(id);
    }

    public bool Add(string id)
    {
        List<string> ids = Ids;
        if (ids == null || string.IsNullOrEmpty(id) || ids.Contains(id)) return false;

        Clue clue = Resolve(id);
        if (!string.IsNullOrEmpty(clue.supersedes)) ids.Remove(clue.supersedes);

        ids.Add(id);
        if (AudioManager.Instance != null) AudioManager.Instance.Play(GameIds.SfxClue);
        Added?.Invoke(clue);
        onChanged?.Invoke();
        return true;
    }

    // Newest first.
    public List<Clue> Entries()
    {
        var list = new List<Clue>();
        List<string> ids = Ids;
        if (ids == null) return list;

        for (int i = ids.Count - 1; i >= 0; i--) list.Add(Resolve(ids[i]));
        return list;
    }

    public Clue Resolve(string id)
    {
        LoadLibrary();
        if (library.TryGetValue(id, out Clue c)) return c;
        return new Clue { id = id, title = Prettify(id), body = "", supersedes = "" };
    }

    private void LoadLibrary()
    {
        if (loaded) return;
        loaded = true;

        string name = clueFileName.EndsWith(".json") ? clueFileName.Substring(0, clueFileName.Length - 5) : clueFileName;

        try
        {
            TextAsset asset = Resources.Load<TextAsset>(name);
            if (asset == null)
            {
                Debug.LogWarning($"Clue file not found in Resources: {name}");
                return;
            }

            ClueFile parsed = JsonUtility.FromJson<ClueFile>(asset.text);
            if (parsed?.clues == null) return;

            foreach (Clue c in parsed.clues)
            {
                if (c == null || string.IsNullOrEmpty(c.id) || library.ContainsKey(c.id)) continue;
                library[c.id] = c;
                catalog.Add(c);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Clue file failed to load: {e.Message}");
        }
    }

    private static string Prettify(string id)
    {
        string s = id.StartsWith("clue_") ? id.Substring(5) : id;
        s = s.Replace('_', ' ');
        if (s.Length > 0) s = char.ToUpper(s[0]) + s.Substring(1);
        return s;
    }

    public bool KnownOrSuperseded(string id)
    {
        if (Has(id)) return true;

        List<string> ids = Ids;
        if (ids == null) return false;

        foreach (string known in ids)
            if (Resolve(known).supersedes == id) return true;

        return false;
    }
}
