// Robber.cs
// Crew member behaviour on top of NPC: following the player along a
// breadcrumb trail (so the escort does not cut corners through walls).
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NPC))]
public class Robber : MonoBehaviour
{
    public bool isLeader;

    [SerializeField] private float followDistance = 2f;
    [SerializeField] private float crumbSpacing = 0.5f;

    private NPC npc;
    private Transform target;
    private readonly List<Vector3> crumbs = new List<Vector3>();
    private Vector3 lastCrumb;

    private void Awake() => npc = GetComponent<NPC>();

    public void Follow(Transform who)
    {
        target = who;
        crumbs.Clear();
        if (who != null) lastCrumb = who.position;
    }

    public void StopFollowing()
    {
        target = null;
        crumbs.Clear();
    }

    private void Update()
    {
        if (target == null || npc.IsMoving || npc.Hidden) return;

        // Drop a crumb whenever the target has moved far enough.
        if ((target.position - lastCrumb).sqrMagnitude >= crumbSpacing * crumbSpacing)
        {
            lastCrumb = target.position;
            crumbs.Add(lastCrumb);
        }

        // Close enough: stand still and forget the trail.
        Vector3 flat = target.position - transform.position;
        flat.y = 0f;
        if (flat.magnitude <= followDistance) { crumbs.Clear(); return; }

        if (crumbs.Count == 0) return;

        Vector3 next = crumbs[0];
        next.y = transform.position.y;
        transform.position = Vector3.MoveTowards(transform.position, next, npc.moveSpeed * Time.deltaTime);

        if ((transform.position - next).sqrMagnitude < 0.01f) crumbs.RemoveAt(0);
    }
}
