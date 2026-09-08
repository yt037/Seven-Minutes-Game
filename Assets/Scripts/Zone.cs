// Zone.cs
// Trigger volume that fires a UnityEvent when the player enters it, subject to
// run flag conditions. With checkWhileInside it also fires the moment a
// missing flag arrives while the player is standing in it.
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class Zone : MonoBehaviour
{
    [SerializeField] private string[] requiredFlags;
    [SerializeField] private string[] blockedFlags;
    [SerializeField] private bool once = true;
    [Tooltip("For repeatable zones: seconds between firings.")]
    [SerializeField] private float cooldownSeconds = 0f;
    [SerializeField] private bool checkWhileInside;
    public UnityEvent onEnter;

    private bool fired;
    private float lastFired = -999f;

    private void Reset() => GetComponent<Collider>().isTrigger = true;

    private void OnTriggerEnter(Collider other) => Try(other);

    private void OnTriggerStay(Collider other)
    {
        if (checkWhileInside) Try(other);
    }

    private void Try(Collider other)
    {
        if (once && fired) return;
        if (Time.time - lastFired < cooldownSeconds) return;
        if (other.GetComponentInParent<Player>() == null) return;
        if (!ConditionsMet()) return;

        fired = true;
        lastFired = Time.time;
        onEnter?.Invoke();
    }

    private bool ConditionsMet()
    {
        GameManager gm = GameManager.Instance;

        if (requiredFlags != null)
            foreach (string f in requiredFlags)
                if (!string.IsNullOrEmpty(f) && (gm == null || !gm.HasFlag(f))) return false;

        if (blockedFlags != null)
            foreach (string f in blockedFlags)
                if (!string.IsNullOrEmpty(f) && gm != null && gm.HasFlag(f)) return false;

        return true;
    }
}
