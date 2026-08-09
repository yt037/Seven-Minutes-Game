using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class NPCEvent
{
    public float time;
    public Transform location;
    public UnityEvent eventCall;
}

public class NPC : MonoBehaviour
{
    public float moveSpeed = 2f;

    public NPCEvent[] events;

    [SerializeField] private Timer timer;

    private int currentIndex = 0;
    private bool moving = false;

    void Update()
    {
        if (currentIndex >= events.Length)
            return;

        NPCEvent currentEvent = events[currentIndex];

        if (!moving && timer.timer <= currentEvent.time)
        {
            StartEvent(currentEvent);
        }

        if (moving)
        {
            MoveToLocation(currentEvent);
        }
    }

    void StartEvent(NPCEvent npcEvent)
    {
        npcEvent.eventCall?.Invoke();

        if (npcEvent.location != null)
        {
            moving = true;
        }
        else
        {
            currentIndex++;
        }
    }

    void MoveToLocation(NPCEvent npcEvent)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            npcEvent.location.position,
            moveSpeed * Time.deltaTime
        );

        if (transform.position == npcEvent.location.position)
        {
            moving = false;
            currentIndex++;
        }
    }
}