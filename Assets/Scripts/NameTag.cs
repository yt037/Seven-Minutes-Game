// NameTag.cs
// Holds an NPC's display name. That is all it does. TalkingNPC uses it for the
// crosshair prompt and NPC.Say uses it for the subtitle speaker label. There is
// no floating label above the head; the hover prompt covers that.
using UnityEngine;

public class NameTag : MonoBehaviour
{
    [SerializeField] private string displayName = "";

    public string DisplayName => displayName;
}