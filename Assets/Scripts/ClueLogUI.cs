// ClueLogUI.cs
// Tab opens the registry: items first, then notes, numbered by catalogue order.
// Entries not yet found read ???. 
//Items in hand this run are marked. Clicking a row fills the detail pane.
//The cursor unlocks while it is open and the clock keeps running.
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClueLogUI : MonoBehaviour
{
    public static ClueLogUI Instance { get; private set; }
    public static bool IsOpen { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private Transform content;
    [SerializeField] private GameObject rowTemplate;
    [SerializeField] private TMP_Text detailTitle;
    [SerializeField] private TMP_Text detailBody;
    [SerializeField] private UIAudio uiAudio;
   

    [Header("Labels")]
    [SerializeField] private string itemsHeader = "ITEMS";
    [SerializeField] private string notesHeader = "NOTES";
    [SerializeField] private string unknownLabel = "???";
    [SerializeField] private string carriedMark = " *";
    [SerializeField] private string promptText = "Select an entry.";
    [SerializeField] private string unknownBody = "Not found yet.";

    [Header("Colours")]
    [SerializeField] private Color headerColour = new Color(0.55f, 0.68f, 0.9f);
    [SerializeField] private Color unknownColour = new Color(0.45f, 0.45f, 0.45f);
    [SerializeField] private Color knownColour = Color.white;
    [SerializeField] private Color carriedColour = new Color(1f, 0.82f, 0.35f);

    private readonly List<GameObject> spawned = new List<GameObject>();
    private ClueLog subscribedTo;
    private string selectedId;
    private bool selectedIsItem;

    private void Awake() => Instance = this;

    private void Start()
    {
        if (panel != null) panel.SetActive(false);
        if (rowTemplate != null) rowTemplate.SetActive(false);
        IsOpen = false;
        Subscribe();
    }

    private void OnDestroy()
    {
        if (subscribedTo != null) subscribedTo.Added -= OnAdded;
        if (Instance == this) Instance = null;
        IsOpen = false;
    }

    private void Update()
    {
        Subscribe();
        if (panel == null || PauseMenu.IsPaused) return;

        if (IsOpen && ShouldForceClose()) { Close(); return; }

        if (Keys.TabPressed)
        {
            if (IsOpen) Close();
            else Open();
        }
    }

    private static bool ShouldForceClose()
    {
        GameManager gm = GameManager.Instance;
        if (gm != null && gm.RunOver) return true;
        return DialogueRunner.Instance != null && DialogueRunner.Instance.IsRunning;
    }

    public void Open()
    {
        IsOpen = true;
        panel.SetActive(true);
        selectedId = null;
        Rebuild();
        ShowDetail(null, null, false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (uiAudio != null)
        uiAudio.PlayToggle();
       
    }

    public void Close()
    {
        IsOpen = false;
        if (panel != null) panel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (uiAudio != null)
        uiAudio.PlayToggle();
      
    }

    // for any Inspector event still pointing here.
    public void Refresh()
    {
        if (IsOpen) Rebuild();
    }

    private void Subscribe()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.Clues == null || subscribedTo == gm.Clues) return;

        if (subscribedTo != null) subscribedTo.Added -= OnAdded;
        subscribedTo = gm.Clues;
        subscribedTo.Added += OnAdded;
    }

    private void OnAdded(Clue clue)
    {
        if (IsOpen) Rebuild();
    }

    private void Rebuild()
    {
        if (content == null || rowTemplate == null) return;

        foreach (GameObject go in spawned) Destroy(go);
        spawned.Clear();

        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        AddHeader(itemsHeader);
        ItemLog items = gm.Items;
        if (items != null)
        {
            int n = 0;
            foreach (Item item in items.Catalog)
            {
                n++;
                bool seen = items.Seen(item.id);
                bool held = seen && items.Held(item.id);
                string label = seen ? item.title + (held ? carriedMark : "") : unknownLabel;
                Color colour = !seen ? unknownColour : held ? carriedColour : knownColour;
                AddRow(n, label, colour, item.id, true, seen);
            }
        }

        AddHeader(notesHeader);
        ClueLog clues = gm.Clues;
        if (clues != null)
        {
            int n = 0;
            foreach (Clue clue in clues.Catalog)
            {
                n++;
                bool known = clues.KnownOrSuperseded(clue.id);
                AddRow(n, known ? clue.title : unknownLabel, known ? knownColour : unknownColour, clue.id, false, known);
            }
        }
    }

    private void AddHeader(string text)
    {
        GameObject row = NewRow();

        TMP_Text label = row.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.text = text;
            label.color = headerColour;
        }

        Button button = row.GetComponent<Button>();
        if (button != null) button.interactable = false;
    }

    private void AddRow(int number, string text, Color colour, string id, bool isItem, bool known)
    {
        GameObject row = NewRow();

        TMP_Text label = row.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            bool selected = id == selectedId && isItem == selectedIsItem;
            label.text = (selected ? "> " : "   ") + number.ToString("00") + "  " + text;
            label.color = colour;
        }

        Button button = row.GetComponent<Button>();
        if (button != null)
        {
            string rowId = id;
            bool rowIsItem = isItem;
            bool rowKnown = known;
            button.onClick.AddListener(() => Select(rowId, rowIsItem, rowKnown));
        }
    }

    private GameObject NewRow()
    {
        GameObject row = Instantiate(rowTemplate, content);
        row.SetActive(true);
        spawned.Add(row);
        return row;
    }

    private void Select(string id, bool isItem, bool known)
    {
        selectedId = id;
        selectedIsItem = isItem;

        string title = null;
        string body = null;
        GameManager gm = GameManager.Instance;

        if (known && gm != null)
        {
            if (isItem && gm.Items != null)
            {
                Item item = gm.Items.Resolve(id);
                if (item != null) { title = item.title; body = item.body; }
            }
            else if (!isItem && gm.Clues != null)
            {
                Clue clue = gm.Clues.Resolve(id);
                title = clue.title;
                body = clue.body;
            }
        }

        ShowDetail(title, body, known);
        Rebuild();
    }

    private void ShowDetail(string title, string body, bool known)
    {
        bool nothingPicked = string.IsNullOrEmpty(selectedId);

        if (detailTitle != null)
            detailTitle.text = nothingPicked ? "" : known ? title : unknownLabel;

        if (detailBody != null)
            detailBody.text = nothingPicked ? promptText : known ? body : unknownBody;
    }
}