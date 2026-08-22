using UnityEngine;

public class NPC : MonoBehaviour
{
    public float moveSpeed = 2f;

    public Transform[] locations;

    public float[] times;

    [SerializeField] private Timer timer;

    private int currentIndex = 0;

    void Update()
    {

        if (currentIndex >= locations.Length)
        {
            return;
        }

        if (timer.timer <= times[currentIndex])
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                locations[currentIndex].position,
                moveSpeed * Time.deltaTime
            );

            // Once we've reached the location, move to the next index
            if (transform.position == locations[currentIndex].position)
            {
                currentIndex++;
            }
        }
    }

<<<<<<< Updated upstream
    //void TriggerEvent()
    //{
    //    Debug.Log("Robbery starts");
    //}
=======
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
    public void SetSchedule(NPCEvent[] newSchedule)
    {
        events = newSchedule;
        currentIndex = 0;
        moving = false;
    }

    public void StartRobberyCall()
    {
        GameManager.Instance.StartRobbery();
    }
>>>>>>> Stashed changes
}