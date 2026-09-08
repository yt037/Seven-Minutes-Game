// Timer.cs
// The run clock and its display, in one place. Stores the time as seconds
// elapsed since 09:59:00 and shows it as a wall clock counting forward.
// 480 seconds total: one pre-heist minute, then the seven minute heist.
//
// Also owns the two smaller countdowns that hang off the clock: the security
// window (the difficulty dial) and the 30 second escort countdown that starts
// when the staff door is opened. Both report through UnityEvents wired in the
// Inspector so choreography stays in the scene, not in code.
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{
    public const float TotalSeconds = 480f;
    public const float HeistAtSecond = 60f;

    [Serializable]
    public class Beat
    {
        public string label;
        public float atSecond;
        public UnityEvent action;
        [NonSerialized] public bool fired;
    }

    [SerializeField] private TextMeshProUGUI text;

    [Header("Security window (the difficulty dial)")]
    [SerializeField] private float securityWindowSeconds = 90f;
    public UnityEvent onWindowEnded;

    [Header("Escort countdown (from opening the staff door)")]
    [SerializeField] private float escortSeconds = 30f;
    [SerializeField] private float escortWarning1At = 10f;
    [SerializeField] private float escortWarning2At = 20f;
    public UnityEvent onEscortWarning1;
    public UnityEvent onEscortWarning2;
    public UnityEvent onEscortExpired;

    [Header("Clock beats")]
    [SerializeField] private Beat[] beats;
    public UnityEvent onExpired;

    public float Elapsed { get; private set; }
    public float Remaining => Mathf.Max(0f, TotalSeconds - Elapsed);
    public bool Running { get; private set; }
    public bool HeistStarted => Running && Elapsed >= HeistAtSecond;

    private float windowEndsAt = -1f;
    private float escortStartedAt = -1f;
    private int escortStage;

    private void Awake()
    {
        if (text != null) text.gameObject.SetActive(false);
        if (beats != null) Array.Sort(beats, (a, b) => a.atSecond.CompareTo(b.atSecond));
    }

    private void Update()
    {
        if (!Running) return;

        Elapsed += Time.deltaTime;
        FireDueBeats();
        UpdateWindow();
        UpdateEscort();
        UpdateClock();

        if (Elapsed >= TotalSeconds)
        {
            Stop();
            onExpired?.Invoke();
        }
    }

    // Called by GameManager.EnterBank when the player crosses the BankEntry zone.
    public void Begin()
    {
        if (Running) return;

        Elapsed = 0f;
        windowEndsAt = -1f;
        escortStartedAt = -1f;
        escortStage = 0;
        if (beats != null) foreach (Beat b in beats) b.fired = false;

        Running = true;
        if (text != null) text.gameObject.SetActive(true);
        UpdateClock();
    }

    public void Stop()
    {
        Running = false;
    }

    // Later runs: talking to a teller jumps straight to 10:00:00. Every beat
    // before that fires now, in order, so the world state matches an unskipped run.
    public void JumpToHeist()
    {
        if (!Running || Elapsed >= HeistAtSecond) return;
        Elapsed = HeistAtSecond;
        FireDueBeats();
        UpdateClock();
    }

    public void StartSecurityWindow()
    {
        if (!Running || windowEndsAt >= 0f) return;
        windowEndsAt = Elapsed + securityWindowSeconds;
    }

    public void StartEscortCountdown()
    {
        if (!Running || escortStartedAt >= 0f) return;
        escortStartedAt = Elapsed;
        escortStage = 0;
    }

    public void CancelEscortCountdown()
    {
        escortStartedAt = -1f;
        escortStage = 3;
    }

    private void FireDueBeats()
    {
        if (beats == null) return;

        foreach (Beat b in beats)
        {
            if (b.fired || b.atSecond > Elapsed) continue;
            b.fired = true;
            b.action?.Invoke();
        }
    }

    private void UpdateWindow()
    {
        if (windowEndsAt < 0f || Elapsed < windowEndsAt) return;
        windowEndsAt = float.MaxValue;   // fire once
        onWindowEnded?.Invoke();
    }

    private void UpdateEscort()
    {
        if (escortStartedAt < 0f) return;
        float since = Elapsed - escortStartedAt;

        if (escortStage == 0 && since >= escortWarning1At) { escortStage = 1; onEscortWarning1?.Invoke(); }
        if (escortStage == 1 && since >= escortWarning2At) { escortStage = 2; onEscortWarning2?.Invoke(); }
        if (escortStage == 2 && since >= escortSeconds) { escortStage = 3; escortStartedAt = -1f; onEscortExpired?.Invoke(); }
    }

    private void UpdateClock()
    {
        if (text == null) return;

        int t = Mathf.FloorToInt(Mathf.Clamp(Elapsed, 0f, TotalSeconds));
        int total = 9 * 3600 + 59 * 60 + t;
        int h = (total / 3600) % 24;
        int m = (total / 60) % 60;
        int s = total % 60;
        text.text = $"{h:00}:{m:00}:{s:00}";
    }
}
