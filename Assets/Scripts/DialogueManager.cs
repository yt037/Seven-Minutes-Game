// DialogueManager.cs
// Owns the dialogue UI widgets and the death flash. Knows nothing about
// dialogue logic; DialogueRunner drives it.
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject dialogue;           // the panel
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI[] choiceTexts; // four slots, keys 1 to 4

    [Header("Death flash")]
    [SerializeField] private Image flashImage;              // full screen, red
    [SerializeField] private float flashSeconds = 0.6f;

    public int ChoiceSlots => choiceTexts != null ? choiceTexts.Length : 0;

    public void OpenDialogue()
    {
        if (dialogue != null && !dialogue.activeSelf) dialogue.SetActive(true);
    }

    public void CloseDialogue()
    {
        if (dialogue != null && dialogue.activeSelf) dialogue.SetActive(false);
        SetSpeaker(null);
        HideChoices();
    }

    public void ChangeDialogueText(string text)
    {
        if (dialogueText != null) dialogueText.text = text ?? "";
    }

    public void SetSpeaker(string speaker)
    {
        if (speakerText == null) return;
        speakerText.text = speaker ?? "";
        speakerText.gameObject.SetActive(!string.IsNullOrEmpty(speaker));
    }

    public void ShowChoices(string[] labels)
    {
        if (choiceTexts == null) return;

        for (int i = 0; i < choiceTexts.Length; i++)
        {
            if (choiceTexts[i] == null) continue;
            bool used = labels != null && i < labels.Length;
            choiceTexts[i].gameObject.SetActive(used);
            if (used) choiceTexts[i].text = $"{i + 1}. {labels[i]}";
        }
    }

    public void HideChoices() => ShowChoices(null);

    // Full screen red, fading out. onDone fires when it has faded.
    public void Flash(Action onDone)
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine(onDone));
    }

    private IEnumerator FlashRoutine(Action onDone)
    {
        if (flashImage != null)
        {
            flashImage.gameObject.SetActive(true);
            Color c = flashImage.color;
            float t = 0f;

            while (t < flashSeconds)
            {
                t += Time.unscaledDeltaTime;
                c.a = Mathf.Lerp(1f, 0f, t / flashSeconds);
                flashImage.color = c;
                yield return null;
            }

            c.a = 0f;
            flashImage.color = c;
            flashImage.gameObject.SetActive(false);
        }

        onDone?.Invoke();
    }
}
