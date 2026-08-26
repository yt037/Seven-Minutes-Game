using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject dialogue;
    [SerializeField] private TextMeshProUGUI dialogueText;

    public void OpenDialogue()
    {
        if (dialogue != null && !(dialogue.activeSelf))
        {
            dialogue.SetActive(true);
        }
    }

    public void CloseDialogue()
    {
        if (dialogue != null && dialogue.activeSelf)
        {
            dialogue.SetActive(false);
        }
    }

    public void ChangeDialogueText(string text)
    {
        if (dialogueText != null)
        {
            dialogueText.text = text;
        }
    }
}
