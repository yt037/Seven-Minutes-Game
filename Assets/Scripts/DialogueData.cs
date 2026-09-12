// DialogueData.cs
// Data classes matching the JSON files in StreamingAssets. JsonUtility needs
// [Serializable] classes with public fields. Unknown keys in the JSON (like
// "_note") are ignored, missing keys become null, empty or 0.
using System;

[Serializable]
public class DialogueFile
{
    public string startNode;            // node used when no startRule matches
    public StartRule[] startRules;      // first rule whose flag is held wins
    public DialogueNode[] nodes;
}

[Serializable]
public class StartRule
{
    public string flag;                 // run flag, persistent flag or clue id
    public string node;                 // node id to open on
}

[Serializable]
public class DialogueNode
{
    public string id;
    public string speaker;
    public string text;
    public string[] textVariants;       // one is picked at random instead of text

    public string[] setFlags;           // run flags set when this node shows
    public string[] setPersistentFlags; // flags kept until the game quits
    public string[] addClues;           // clue ids added to the clue log
    public string sound;

    public DialogueChoice[] choices;    // if empty, E advances to next
    public string next;                 // node id, or empty to end
}

[Serializable]
public class DialogueChoice
{
    public string text;
    public string next;

    public string[] requiredFlags;      // hidden unless all are held
    public string[] blockedFlags;       // hidden if any is held
    public int minSuspicion;            // hidden until the counter reaches this

    public string[] setFlags;
    public string[] setPersistentFlags;
    public int suspicion;               // added to the conversation counter
    public string ending;               // "failure" ends the run fatally; other values are ignored
}

// clues.json
[Serializable]
public class ClueFile
{
    public Clue[] clues;
}

[Serializable]
public class Clue
{
    public string id;
    public string title;
    public string body;
    public string supersedes;           // clue id removed when this one is added
}

// items.json
[Serializable]
public class ItemFile
{
    public Item[] items;
}

[Serializable]
public class Item
{
    public string id;                   // the run flag the pickup grants
    public string title;
    public string body;
}