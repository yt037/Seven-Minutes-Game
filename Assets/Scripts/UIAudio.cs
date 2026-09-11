using UnityEngine;

public class UIAudio : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip buttonClick;
    public AudioClip respawn;
    public AudioClip toggle;

    public void PlayButtonClick()
    {
        audioSource.PlayOneShot(buttonClick);
    }

    public void PlayRespawn()
    {
        audioSource.PlayOneShot(respawn);
    }

    public void PlayToggle()
    {
        audioSource.PlayOneShot(toggle);
    }
}