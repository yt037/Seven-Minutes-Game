// SubtitleLog.cs
// Rolling subtitle strip for NPC speech bubbles. A line arrives from NPC.Say,
// holds for holdSeconds, fades over fadeSeconds, then drops off. Dialogue box
// lines are not fed in here; they are already centre screen and advance-locked.
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class SubtitleLog : MonoBehaviour
{
    public static SubtitleLog Instance { get; private set; }

    [SerializeField] private TMP_Text text;
    [SerializeField] private int maxLines = 4;
    [SerializeField] private float holdSeconds = 6f;
    [SerializeField] private float fadeSeconds = 1.5f;
    [SerializeField] private bool hideDuringDialogue = true;

    private struct Line
    {
        public string speaker;
        public string body;
        public float bornAt;
    }

    private readonly List<Line> lines = new List<Line>();
    private readonly StringBuilder sb = new StringBuilder();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        if (text == null) text = GetComponent<TMP_Text>();
        if (text != null) text.text = "";
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Add(string speaker, string body)
    {
        if (text == null || string.IsNullOrEmpty(body)) return;

        lines.Add(new Line { speaker = speaker, body = body, bornAt = Time.unscaledTime });
        while (lines.Count > maxLines) lines.RemoveAt(0);
    }

    public void Clear() => lines.Clear();

    private void Update()
    {
        if (text == null) return;

        float life = holdSeconds + fadeSeconds;
        for (int i = lines.Count - 1; i >= 0; i--)
            if (Time.unscaledTime - lines[i].bornAt >= life) lines.RemoveAt(i);

        bool hidden = hideDuringDialogue && DialogueRunner.Instance != null && DialogueRunner.Instance.IsRunning;
        if (hidden || lines.Count == 0)
        {
            if (text.text.Length > 0) text.text = "";
            return;
        }

        sb.Clear();
        for (int i = 0; i < lines.Count; i++)
        {
            Line l = lines[i];
            float age = Time.unscaledTime - l.bornAt;
            float alpha = age <= holdSeconds
                ? 1f
                : 1f - Mathf.Clamp01((age - holdSeconds) / Mathf.Max(fadeSeconds, 0.01f));

            sb.Append("<alpha=#").Append(Mathf.RoundToInt(alpha * 255f).ToString("X2")).Append('>');
            if (!string.IsNullOrEmpty(l.speaker)) sb.Append(l.speaker).Append(": ");
            sb.Append(l.body);
            if (i < lines.Count - 1) sb.Append('\n');
        }
        sb.Append("<alpha=#FF>");

        text.text = sb.ToString();
    }
}