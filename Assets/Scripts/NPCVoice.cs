using UnityEngine;

public enum NPCVoiceType
{
    Male,
    Female,
    Robber
}

public class NPCVoice : MonoBehaviour
{
    [Header("Voice Type")]
    [SerializeField] private NPCVoiceType voiceType;

    [Header("Normal NPC Voices")]
    [SerializeField] private AudioClip[] normalVoices;

    [Header("Robber Voice")]
    [SerializeField] private AudioClip robberVoice;

    private int lastIndex = -1;

    public void PlayVoice()
    {
        AudioClip clip = null;

        if (voiceType == NPCVoiceType.Robber)
        {
            // Robber always uses the same voice
            clip = robberVoice;
        }
        else if (normalVoices != null && normalVoices.Length > 0)
        {
            // Random voice, avoiding immediate repeats
            int index;

            if (normalVoices.Length == 1)
            {
                index = 0;
            }
            else
            {
                do
                {
                    index = Random.Range(0, normalVoices.Length);
                }
                while (index == lastIndex);
            }

            lastIndex = index;
            clip = normalVoices[index];
        }

        if (clip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlayClip(clip);
    }
}