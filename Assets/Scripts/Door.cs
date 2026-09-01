using System.Collections.Generic;
using UnityEngine;

public enum DoorSecurity
{
    Free,
    StaffOnly,
    Keycard,
    Restricted,
    Vault
}

public class Door : MonoBehaviour, Interaction
{
    [SerializeField] private DialogueManager dialogue;
    [SerializeField] private DoorSecurity secLevel;
    [SerializeField] private Animator animator;

    private readonly HashSet<Collider> allowedEntities = new();

    public void Interact(Player player)
    {
        if (dialogue != null)
        {
            dialogue.OpenDialogue();

            switch (secLevel)
            {
                case DoorSecurity.Free:
                    break;

                case DoorSecurity.Keycard:
                    dialogue.ChangeDialogueText("This door has a keycard slot! Seems like a keycard is necessary to unlock it.");
                    break;

                case DoorSecurity.StaffOnly:
                    dialogue.ChangeDialogueText("This door is employee-only! Seems like only employees can enter.");
                    break;

                case DoorSecurity.Restricted:
                    dialogue.ChangeDialogueText("This door is restricted! Seems like only authorized personnel can enter.");
                    break;

                case DoorSecurity.Vault:
                    dialogue.ChangeDialogueText("This vault door has heavy security! Seems like a keycard is required to even try to open it.");
                    break;

                default:
                    break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //.Log(other + " Entered");
        if (!IsAllowed(other))
        {
            return;
        }

        allowedEntities.Add(other);
        animator.SetBool("Passable", true);
    }

    private void OnTriggerExit(Collider other)
    {
        //Debug.Log(other + " Exited");
        if (!allowedEntities.Contains(other))
        {
            return;
        }

        allowedEntities.Remove(other);

        if (allowedEntities.Count == 0)
        {
            animator.SetBool("Passable", false);
        }
    }

    private bool IsAllowed(Collider other)
    {
        //if (!other.CompareTag("Player") && !other.CompareTag("NPC"))
        //{
        //    return false;
        //}

        switch (secLevel)
        {
            case DoorSecurity.Free:
                return true;

            case DoorSecurity.Keycard:
                return other.CompareTag("Player") && GameManager.Instance.HasKeycard;

            case DoorSecurity.StaffOnly:
                return (other.CompareTag("Player") && GameManager.Instance.HasKeycard) || other.CompareTag("Manager") || other.CompareTag("Guard") || other.CompareTag("Employee");

            case DoorSecurity.Restricted:
                return (other.CompareTag("Player") && GameManager.Instance.HasKeycard) || other.CompareTag("Manager") || other.CompareTag("Guard");

            case DoorSecurity.Vault:
                return (other.CompareTag("Player") && GameManager.Instance.HasKeycard) || (other.CompareTag("Manager"));

            default:
                return false;
        }
    }
}
