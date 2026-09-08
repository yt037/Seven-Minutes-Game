// Guard.cs
// Bank guard. The manager orders them to stand down at 10:01.
using UnityEngine;

[RequireComponent(typeof(NPC))]
public class Guard : MonoBehaviour
{
    [SerializeField] private string standDownLine = "Alright. Alright. Hands up.";

    public bool StoodDown { get; private set; }

    public void StandDown()
    {
        StoodDown = true;
        NPC npc = GetComponent<NPC>();
        if (npc != null)
        {
            npc.SetBlocking(false);
            npc.Say(standDownLine);
        }
    }
}
