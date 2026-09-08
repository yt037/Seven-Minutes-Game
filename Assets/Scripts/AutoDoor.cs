// AutoDoor.cs
// Sliding door with a trigger volume covering the doorway. Tracks who is
// standing in it so it does not slam on the second person through.
// NPCs always open it. The player opens it only while it is unlocked.
// KeycardReader unlocks it; the CorridorEntry zone and the escort countdown
// lock it again behind the player.
using System.Collections.Generic;
using UnityEngine;

public class AutoDoor : MonoBehaviour, Interaction
{
    [SerializeField] private Animator animator;
    [SerializeField] private string openTriggerName = "Open";
    [SerializeField] private string closeTriggerName = "Close";

    [Tooltip("Only objects on these layers open the door.")]
    [SerializeField] private LayerMask openedBy = ~0;

    [Tooltip("Seconds the doorway must stay empty before closing.")]
    [SerializeField] private float closeDelay = 1f;

    [Header("Player access")]
    [SerializeField] private string displayName = "Door";
    [SerializeField] private bool startLocked;
    [SerializeField] private string lockedLine = "Locked.";

    private readonly HashSet<Collider> occupants = new HashSet<Collider>();
    private bool isOpen;
    private float emptySince;

    public bool IsOpen => isOpen;
    public bool LockedForPlayer { get; private set; }

    public string Prompt => displayName;

    private void Reset() => animator = GetComponent<Animator>();

    private void Awake() => LockedForPlayer = startLocked;

    public void Unlock() => LockedForPlayer = false;

    public void LockForPlayer()
    {
        LockedForPlayer = true;
        occupants.RemoveWhere(IsPlayerCollider);
    }

    public void Interact(Player player)
    {
        if (LockedForPlayer && !string.IsNullOrEmpty(lockedLine) && DialogueRunner.Instance != null)
            DialogueRunner.Instance.ShowLine(lockedLine);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsAccepted(other)) return;

        occupants.Add(other);
        if (!isOpen) Open();
    }

    private void OnTriggerStay(Collider other)
    {
        // A player standing in the doorway when it unlocks should open it.
        if (isOpen || !IsAccepted(other)) return;
        occupants.Add(other);
        Open();
    }

    private void OnTriggerExit(Collider other) => occupants.Remove(other);

    private void Update()
    {
        if (!isOpen) return;

        occupants.RemoveWhere(c => c == null || !c.gameObject.activeInHierarchy);

        if (occupants.Count > 0)
        {
            emptySince = Time.time;
            return;
        }

        if (Time.time - emptySince >= closeDelay) Close();
    }

    private bool IsAccepted(Collider other)
    {
        if ((openedBy.value & (1 << other.gameObject.layer)) == 0) return false;
        if (LockedForPlayer && IsPlayerCollider(other)) return false;
        return true;
    }

    private static bool IsPlayerCollider(Collider c) => c != null && c.GetComponentInParent<Player>() != null;

    private void Open()
    {
        isOpen = true;
        emptySince = Time.time;
        if (animator != null) animator.SetTrigger(openTriggerName);
    }

    private void Close()
    {
        isOpen = false;
        if (animator != null) animator.SetTrigger(closeTriggerName);
    }
}
