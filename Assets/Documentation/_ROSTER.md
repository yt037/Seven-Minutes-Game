# NPC roster and wiring checklist

16 NPCs. Drop all `.json` files into `Assets/StreamingAssets/Dialogue/`.
Set each NPC's dialogue file field to the filename without `.json`.

## Hierarchy to build

```
NPCs
├── BankManager
├── Robbers
│   ├── RobberLeader
│   └── RobberGoon_1 .. RobberGoon_4
├── Guards
│   ├── GuardSecurity
│   ├── GuardLobby_1
│   └── GuardLobby_2
├── Tellers
│   ├── Teller_1
│   └── Teller_2
└── Customers
    └── Customer_1 .. Customer_5

SpawnPoints          (empties, one per NPC, same names)
```

Build `SpawnPoints` as empties now and leave `NPCs` at origin.
When the layout lands, move 16 empties instead of 16 configured NPCs.

## Every NPC needs

- BoxCollider (raycast target, this is what was missing on Bank Manager before)
- NavMeshAgent (if it moves)
- The NPC/interactable component with its dialogue file id

## Item pickups that MUST set flags

The dialogue gates lie-breaking on these. Nothing works until Card.cs sets them.

| Item | Flag | Location it should sit |
|---|---|---|
| vault_override_log | `has_vault_override_log` | Records room |
| keycard_log | `has_keycard_log` | Security room terminal |
| driver_photo | `has_driver_photo` | Break room |
| debt_notice | `has_debt_notice` | Manager's office desk |

## Clue threads

| NPC | Points at | Persistent flag set |
|---|---|---|
| guard_security | keycard_log | knows_backdoor_swipe |
| teller_2 | vault_override_log | knows_override_exists |
| teller_1 | debt_notice | knows_manager_letters |
| customer_3 | driver_photo | knows_driver_face |
| robber_leader | nothing, gives testimony | knows_robbers_had_insider |

## The contradiction

`guard_lobby_2` says the robbers came in after nine.
`robber_leader` and the camera log say before.
This is intentional. The player must find the conflict themselves.

## Adding dialogue

Every file has a `_note` on each node and an `_extend` block at the bottom.
`_note` and `_extend` are ignored by JsonUtility, so they are safe comments.

To add a node: copy any node object, give it a unique `id`, and point some
choice's `next` at it. Empty `next` ends the conversation.

Validate before testing. Paste into jsonlint.com, or run:
`python3 -c "import json;json.load(open('file.json'))"`
