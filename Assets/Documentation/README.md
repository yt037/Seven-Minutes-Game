# Dialogue file format (for content writers)

Each NPC gets one `.json` file in this folder. Edit with any text editor.
No Unity needed. If the game says "file not found" or "malformed", check
for a missing comma or quote (paste the file into jsonlint.com to check).

## Structure

- `startNode`: id of the node the conversation opens on.
- `nodes`: list of dialogue nodes.

## Node fields

| Field | Meaning |
|---|---|
| `id` | Unique name, referenced by `next` |
| `speaker` | Name shown above the text |
| `text` | What they say |
| `choices` | Player options (max 4 shown). Omit for linear lines |
| `next` | Node to go to when E is pressed (linear nodes). Empty string ends the conversation |
| `setFlags` | Flags set when the node appears (reset each loop) |
| `setPersistentFlags` | Knowledge flags, kept across loops |
| `addClues` | Clue ids added to the clue log (Tab) |

## Choice fields

| Field | Meaning |
|---|---|
| `text` | Button label (player presses 1, 2, 3 or 4) |
| `next` | Node to jump to. Empty string ends the conversation |
| `requiredFlags` | Choice hidden unless ALL of these are set |
| `blockedFlags` | Choice hidden if ANY of these is set |
| `minSuspicion` | Hidden until the suspicion counter reaches this (counter resets every conversation) |
| `suspicion` | Added to the counter when picked |
| `setFlags` / `setPersistentFlags` | Flags set when picked |
| `ending` | Immediately triggers this ending (e.g. "escape") |

Flag and ending ids must match the constants in `Assets/Scripts/GameIds.cs`.
Ask the programmer to add new ids there first.

Remember: the 7-minute timer keeps running while people talk. Keep lines short.
