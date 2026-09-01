using TMPro;
using UnityEngine;

public class Guard : MonoBehaviour, Interaction
{
    [SerializeField] private Robber robber;
    [SerializeField] private DialogueManager dialogue;
    [SerializeField] private int importance;
    [SerializeField] private string[] dialogueLinesCalm;
    [SerializeField] private string[] dialogueLinesAlert;
    [SerializeField] private string[] dialogueLinesHeist;

    public void Interact(Player player)
    {
        if (dialogue != null)
        {
            dialogue.OpenDialogue();
        }

        switch (importance) 
        {
            case 1:
                if (dialogue == null)
                {
                    Debug.LogWarning("Guard has no DialogueManager assigned.");
                    return;
                }

                if (GameManager.Instance.HasKeycard)
                {
                    dialogue.ChangeDialogueText("Oh! That's an evil person! I'll handle him");
                    dialogue.OpenDialogue();

                    if (robber != null)
                    {
                        robber.StopRobber();
                    }

                    GameManager.Instance.WarnGuard();

                    Debug.Log("The guard has been warned.");
                }
                else
                {
                    dialogue.ChangeDialogueText("Hey! I will need evidence before I detain any random person");
                    dialogue.OpenDialogue();

                    Debug.Log("Find evidence first.");
                }
                break;

            case 2:
                if (dialogue == null)
                {
                    Debug.LogWarning("Guard has no DialogueManager assigned.");
                    return;
                }

                if (GameManager.Instance.RobberyStarted == true)
                {
                    if (dialogueLinesHeist.Length > 0)
                    {
                        int randomIndex = Random.Range(0, dialogueLinesHeist.Length);
                        dialogue.ChangeDialogueText(dialogueLinesHeist[randomIndex]);
                        dialogue.OpenDialogue();
                    }
                }
                else if (GameManager.Instance.GuardWarned == true)
                {
                    if (dialogueLinesAlert.Length > 0)
                    {
                        int randomIndex = Random.Range(0, dialogueLinesAlert.Length);
                        dialogue.ChangeDialogueText(dialogueLinesAlert[randomIndex]);
                        dialogue.OpenDialogue();
                    }
                }
                else
                {
                    if (dialogueLinesCalm.Length > 0)
                    {
                        int randomIndex = Random.Range(0, dialogueLinesCalm.Length);
                        dialogue.ChangeDialogueText(dialogueLinesCalm[randomIndex]);
                        dialogue.OpenDialogue();
                    }
                }
                break;
        }
    }
}
