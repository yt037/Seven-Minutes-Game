// NPC.cs
// The puppet. Every character is one of these. It can walk to a named post
// along a waypoint path, appear, vanish, fall over, block a doorway and show
// a speech bubble. It has no schedule of its own: Timer beats, Zones, readers
// and FlagListeners tell it what to do and when.
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class NPC : MonoBehaviour
{
    [Serializable]
    public class Post
    {
        public string name;
        [Tooltip("Waypoints in order. The last one is the post itself.")]
        public Transform[] path;
        [Tooltip("Run flag set on arrival, e.g. leader_at_vault.")]
        public string arrivalFlag;
        public bool blockOnArrive;
        public bool hideOnArrive;
        public string arrivalLine;
        public UnityEvent onArrive;
    }

    public float moveSpeed = 2f;

    [SerializeField] private bool startHidden;
    [SerializeField] private Post[] posts;

    [Header("Speech bubble")]
    [SerializeField] private TMP_Text bubble;
    [SerializeField] private float bubbleSeconds = 5f;
    [SerializeField] private float bubbleHeight = 1.5f;

    public bool Blocking { get; private set; }
    public bool IsMoving => current != null;
    public bool Hidden { get; private set; }
    public bool Collapsed { get; private set; }

    private Post current;
    private int step;
    private Renderer[] renderers;
    private Collider[] colliders;
    private float bubbleUntil;

    private void Awake()
    {
        // Trigger events need a Rigidbody on one side. NPCs move by transform,
        // so a kinematic body makes doors and zones see them without physics.
        Rigidbody body = GetComponent<Rigidbody>();
        if (body == null) body = gameObject.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        renderers = GetComponentsInChildren<Renderer>(true);
        colliders = GetComponentsInChildren<Collider>(true);

        if (bubble == null)
        {
            Transform t = transform.Find("Bubble");
            if (t != null) bubble = t.GetComponent<TMP_Text>();
        }

        if (bubble != null)
        {
            bubble.transform.localPosition = new Vector3(0f, bubbleHeight, 0f);
            bubble.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        if (startHidden) Hide();
    }

       private void Update()
    {
        if (bubble != null && bubble.gameObject.activeSelf && Time.time >= bubbleUntil)
            bubble.gameObject.SetActive(false);

        if (current == null) return;

        Transform target = current.path[step];

        Vector3 destination = target.position;
        destination.y = transform.position.y;
        transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);

        if ((transform.position - destination).sqrMagnitude > 0.0001f) return;

        step++;
        if (step < current.path.Length) return;

        Post arrived = current;
        current = null;
        Arrive(arrived);
    }

    // ---------------------------------------------------------------- commands (UnityEvent friendly)

    public void GoToPost(string postName)
    {
        Post p = Find(postName);
        if (p == null || p.path == null || p.path.Length == 0) return;

        Blocking = false;
        current = p;
        step = 0;
    }

        public void WarpToPost(string postName)
    {
        Post p = Find(postName);
        if (p == null || p.path == null || p.path.Length == 0) return;

        current = null;
        Blocking = false;

        Vector3 destination = p.path[p.path.Length - 1].position;
        destination.y = transform.position.y;
        transform.position = destination;

        Arrive(p);
    }

    public void Say(string text)
    {
        if (bubble == null || string.IsNullOrEmpty(text) || Hidden) return;

        if (SubtitleLog.Instance != null)
        {
            NameTag tag = GetComponent<NameTag>();
            SubtitleLog.Instance.Add(tag != null ? tag.DisplayName : name, text);
        }

        bubble.text = text;
        bubble.gameObject.SetActive(true);
        bubbleUntil = Time.time + Mathf.Max(bubbleSeconds, 2f + text.Length * 0.05f);
    }

    public void Show()
    {
        Hidden = false;
        foreach (Renderer r in renderers) if (r != null) r.enabled = true;
        foreach (Collider c in colliders) if (c != null) c.enabled = true;
        SetTagVisible(true);
    }

    public void Hide()
    {
        Hidden = true;
        Blocking = false;
        if (bubble != null) bubble.gameObject.SetActive(false);
        foreach (Renderer r in renderers) if (r != null) r.enabled = false;
        foreach (Collider c in colliders) if (c != null) c.enabled = false;
        SetTagVisible(false);
    }

    // Shot. Lies flat, stops being talkable, stops facing the camera.
    public void Collapse()
    {
        Debug.Log($"{name} collapsed");
        Collapsed = true;
        current = null;
        Blocking = false;
        if (bubble != null) bubble.gameObject.SetActive(false);

        Billboard bb = GetComponent<Billboard>();
        if (bb != null) bb.enabled = false;

        TalkingNPC talk = GetComponent<TalkingNPC>();
        if (talk != null) talk.enabled = false;

        SetTagVisible(false);
        transform.rotation = Quaternion.Euler(90f, transform.eulerAngles.y, 0f);
        transform.position = new Vector3(transform.position.x, 0.05f, transform.position.z);
    }

    public void SetBlocking(bool value) => Blocking = value;

    // ---------------------------------------------------------------- internals

    private void Arrive(Post p)
    {
        if (p.blockOnArrive) Blocking = true;
        if (!string.IsNullOrEmpty(p.arrivalFlag) && GameManager.Instance != null) GameManager.Instance.SetFlag(p.arrivalFlag);
        if (!string.IsNullOrEmpty(p.arrivalLine)) Say(p.arrivalLine);
        p.onArrive?.Invoke();
        if (p.hideOnArrive) Hide();
    }

    private Post Find(string postName)
    {
        if (posts == null) return null;
        foreach (Post p in posts) if (p != null && p.name == postName) return p;
        Debug.LogWarning($"{name} has no post named '{postName}'.");
        return null;
    }

    private void SetTagVisible(bool value)
    {
        NameTag tag = GetComponent<NameTag>();
        if (tag != null) tag.enabled = value;
    }
}
