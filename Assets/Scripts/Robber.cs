using UnityEngine;

public class Robber : MonoBehaviour
{
    [SerializeField] private NPC npc;

    [SerializeField] private NPCEvent[] normalSchedule;
    [SerializeField] private NPCEvent[] stoppedSchedule;

    public void StopRobber()
    {
        npc.SetSchedule(stoppedSchedule);
    }

    public void Robbery()
    {
        GameManager.Instance.StartRobbery();
    }
}
