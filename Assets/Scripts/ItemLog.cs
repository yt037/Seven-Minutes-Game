// ItemLog.cs
// On the GameManager object, next to ClueLog. Loads the item catalogue from
// StreamingAssets/items.json. An item is Seen once it has been picked up in any
// run this session, and Held while its flag is set in the current run.
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ItemLog : MonoBehaviour
{
    [SerializeField] private string itemFileName = "items.json";

    private readonly List<Item> catalog = new List<Item>();
    private readonly Dictionary<string, Item> byId = new Dictionary<string, Item>();
    private GameManager owner;
    private bool loaded;

    public IReadOnlyList<Item> Catalog { get { Load(); return catalog; } }

    private void Awake()
    {
        Load();
        owner = GetComponent<GameManager>();
        if (owner != null) owner.OnFlagSet += OnFlagSet;
    }

    private void OnDestroy()
    {
        if (owner != null) owner.OnFlagSet -= OnFlagSet;
    }

    private void OnFlagSet(string flag)
    {
        GameManager live = GameManager.Instance;
        if (live != null && byId.ContainsKey(flag)) live.State.itemsSeen.Add(flag);
    }

    public bool Seen(string id)
    {
        GameManager live = GameManager.Instance;
        return live != null && !string.IsNullOrEmpty(id) && live.State.itemsSeen.Contains(id);
    }

    public bool Held(string id)
    {
        GameManager live = GameManager.Instance;
        return live != null && live.HasFlag(id);
    }

    public Item Resolve(string id)
    {
        Load();
        return byId.TryGetValue(id, out Item item) ? item : null;
    }

    private void Load()
    {
        if (loaded) return;
        loaded = true;

        string name = itemFileName.EndsWith(".json") ? itemFileName.Substring(0, itemFileName.Length - 5) : itemFileName;

        try
        {
            TextAsset asset = Resources.Load<TextAsset>(name);
            if (asset == null)
            {
                Debug.LogWarning($"Item file not found in Resources: {name}");
                return;
            }

            ItemFile parsed = JsonUtility.FromJson<ItemFile>(asset.text);
            if (parsed?.items == null) return;

            foreach (Item item in parsed.items)
            {
                if (item == null || string.IsNullOrEmpty(item.id) || byId.ContainsKey(item.id)) continue;
                byId[item.id] = item;
                catalog.Add(item);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Item file failed to load: {e.Message}");
        }
    }
}