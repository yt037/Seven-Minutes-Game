using UnityEngine;

public class Utilities : MonoBehaviour
{
    [SerializeField] private GameObject image;
    [SerializeField] private string flag = GameIds.HasExitCard;

    private void Update()
    {
        if (image == null) return;

        GameManager gm = GameManager.Instance;
        bool held = gm != null && gm.HasFlag(flag);
        if (image.activeSelf != held) image.SetActive(held);
    }

    public void Escape()
    {
        if (GameManager.Instance != null) GameManager.Instance.SteppedOutside();
    }
}