using UnityEngine;

public class Card : MonoBehaviour
{
    public void Collect(Player player)
    {
        GameManager.Instance.CollectKeycard();
        Destroy(gameObject);
    }
}