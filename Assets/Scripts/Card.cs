using UnityEngine;

public class Card : MonoBehaviour, Interaction
{
    public void Interact(Player player)
    {
        GameManager.Instance.CollectKeycard();
        Destroy(gameObject);
    }
}