// FlagListener.cs
// Bridges a run flag to a UnityEvent. Dialogue sets the flag, the world reacts.
using UnityEngine;
using UnityEngine.Events;

public class FlagListener : MonoBehaviour
{
    [SerializeField] private string flag = "";
    [SerializeField] private bool onlyOnce = true;
    [SerializeField] private UnityEvent response;

    private bool fired;
    private GameManager subscribedTo;

    private void Start() => Subscribe();

    private void OnEnable() => Subscribe();

    private void OnDisable() => Unsubscribe();

    private void Subscribe()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || subscribedTo == gm) return;

        Unsubscribe();
        subscribedTo = gm;
        gm.OnFlagSet += HandleFlagSet;
    }

    private void Unsubscribe()
    {
        if (subscribedTo != null) subscribedTo.OnFlagSet -= HandleFlagSet;
        subscribedTo = null;
    }

    private void HandleFlagSet(string setFlag)
    {
        if (setFlag != flag) return;
        if (fired && onlyOnce) return;

        fired = true;
        response?.Invoke();
    }
}
