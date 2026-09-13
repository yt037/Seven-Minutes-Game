using System.Collections;
using UnityEngine;

public class GunshotEventAudio : MonoBehaviour
{
    [SerializeField] private float collapseDelay = 0.1f;

    private NPC npc;

    private void Awake()
    {
        npc = GetComponent<NPC>();
    }

    public void PlayScream()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(GameIds.SfxScream);
    }

    public void PlayGunshotAndCollapse()
    {
        StartCoroutine(GunshotRoutine());
    }

    private IEnumerator GunshotRoutine()
    {
       
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(GameIds.SfxGunshot);

       
        yield return new WaitForSeconds(collapseDelay);

       
        if (npc != null)
            npc.Collapse();
    }
}