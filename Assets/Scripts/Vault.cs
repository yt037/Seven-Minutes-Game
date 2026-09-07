// Vault.cs
// The vault room. Opens when the security window ends, runs the confrontation
// when the player walks in, and puts the moneybag out if the leader offers it.
using UnityEngine;

public class Vault : MonoBehaviour
{
    [SerializeField] private Door door;
    [SerializeField] private Card moneybag;   // starts inactive in the scene

    // Wired to Timer.onWindowEnded.
    public void Open()
    {
        if (GameManager.Instance != null) GameManager.Instance.SetFlag(GameIds.VaultOpen);
        if (door != null) door.Open();
    }

    // Wired to the VaultEntry zone.
    public void OnPlayerEntered()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.RunOver || DialogueRunner.Instance == null) return;

        string reference = gm.HasFlag(GameIds.AlarmTriggered) ? GameIds.DlgVaultAlarm : GameIds.DlgVault;
        DialogueRunner.Instance.PlayForced(reference);
    }

    // Wired to a FlagListener on bag_offered.
    public void OfferBag()
    {
        if (moneybag != null) moneybag.gameObject.SetActive(true);
    }
}
