using TMPro;
using UnityEngine;

public class Guard : MonoBehaviour
{
    [SerializeField] private GameObject Robber;
    [SerializeField] private TextMeshProUGUI dialogue;

    public void Interact(Player player)
    {
        if (GameManager.Instance.HasKeycard)
        {
            dialogue.text = "Oh! Thats an evil person! I'll handle him";
            GameManager.Instance.WarnGuard();
            Debug.Log("The guard has been warned.");
            Destroy(Robber);
        }
        else
        {
            dialogue.text = "Hey! I will need evidence before I detain any random person";
            Debug.Log("Find evidence first.");
        }
    }
}
