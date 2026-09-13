// EndingScreen.cs
// Lives in each ending scene. Works out which ending the scene is from its
// name, reads the last result from GameManager for variant and cause, and
// fills the title and body from the copy table below. Buttons: Continue or
// Try again restarts the run, Main menu returns to the house.
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndingScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Fallback when opened without a GameManager")]
    [SerializeField] private string title = "YOU DIED";
    [TextArea(3, 8)][SerializeField] private string description = "";

    [Header("Buttons")]
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private UIAudio uiAudio;

    private void Start()
    {
        if (AudioManager.Instance != null)
        AudioManager.Instance.StopMusic();
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        string ending = EndingSystem.EndingForScene(SceneManager.GetActiveScene().name);
        GameManager gm = GameManager.Instance;
        EndingResult result = gm != null ? gm.State.lastResult : null;

        if (!string.IsNullOrEmpty(ending))
        {
            string variant = result != null && result.ending == ending ? result.variant : "";
            string cause = result != null && result.ending == ending ? result.cause : "";
            title = TitleFor(ending);
            description = BodyFor(ending, variant, cause);
            PlayEndingAudio(ending);

            if (variant == GameIds.VariantSirens && AudioManager.Instance != null) AudioManager.Instance.Play(GameIds.SfxSirens);
        }

        if (titleText != null) titleText.text = title;
        if (descriptionText != null) descriptionText.text = description;

        if (retryButton != null) retryButton.onClick.AddListener(OnRetryPressed);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuPressed);
    }

    public void OnRetryPressed()
    {
        StartCoroutine(RetryWithSound());
    }

    private System.Collections.IEnumerator RetryWithSound()
    {
    if (uiAudio != null)
    {
        uiAudio.PlayRespawn();

        if (uiAudio.respawn != null)
            yield return new WaitForSecondsRealtime(uiAudio.respawn.length);
        else
            yield return new WaitForSecondsRealtime(0.2f);
    }
    else
    {
        yield return new WaitForSecondsRealtime(0.2f);
    }

    if (GameManager.Instance != null)
        GameManager.Instance.Restart();
    else
        SceneManager.LoadScene(GameIds.SceneBank);
    }

    public void OnMainMenuPressed()
    {
        if (GameManager.Instance != null) GameManager.Instance.GoToMenu();
        else SceneManager.LoadScene(GameIds.SceneMainMenu);
    }

    public void OnRewardDismissed() => OnMainMenuPressed();

    private static string TitleFor(string ending)
    {
        switch (ending)
        {
            case GameIds.EndingFailure: return "YOU DIED";
            case GameIds.EndingEscape: return "ESCAPE";
            case GameIds.EndingHero: return "HERO";
            case GameIds.EndingCriminal: return "CRIMINAL";
            case GameIds.EndingTrue: return "TRUE";
            default: return "";
        }
    }

    private static string BodyFor(string ending, string variant, string cause)
    {
        bool sirens = variant == GameIds.VariantSirens;

        switch (ending)
        {
            case GameIds.EndingFailure:
                switch (cause)
                {
                    case GameIds.CauseDialogue: return "Wrong answer. Vander does not repeat himself.";
                    case GameIds.CauseFrontDoor: return "Thought you could just leave? Think again.";
                    case GameIds.CauseDeadline: return "Thirty seconds. Did you really need to take so long?";
                    case GameIds.CauseExpiry: return "The clock ran out. Did you fall asleep?";
                    case GameIds.CauseTaserNoAlarm: return "Can't win alone? Try calling for backup, the cops will surely oblige.";
                    default: return "You died.";
                }

            case GameIds.EndingEscape:
                return "The side door opened for you like it had been waiting. Six steps out the doorway was the farthest you made before it all went dark.";

            case GameIds.EndingHero:
                return "The alarm did its job. Vander did his: he had you on the floor before the taser cleared your pocket, but he surrendered when the cops surrounded him.\n\n"
                    + "You get a paragraph in the local paper. Three weeks later a man you never met visits the ward after hours. That was the last thing you ever saw.";

            case GameIds.EndingCriminal:
                return "Outside, the manager was waiting, of course it was him. It had to be him.\n"
                    + "A bag of money is a heavy thing to be found holding, but Mr Wilco seemed to have everything arranged. You never got your share of the money and your face is on the news. \n"
                    + "Two weeks on the run and you find yourself in a cell for the next ten years.";

            case GameIds.EndingTrue:
                return "You finally reached the truth!\n"
                    + "After stalling for time, the robber were promptly arrested. The note you found in Mr Wilco's office was a diary of his crimes.\n"
                    + "You did not get to see his arrest in the hospital but the city mayor visited you the next day to thank you for your efforts.";

            default:
                return "";
        }
    }

    private void PlayEndingAudio(string ending)
    {
    if (AudioManager.Instance == null)
        return;

    switch (ending)
    {
        case GameIds.EndingFailure:
            AudioManager.Instance.Play(GameIds.SfxEndingFailure);
            break;

        case GameIds.EndingEscape:
            AudioManager.Instance.Play(GameIds.SfxEndingEscape);
            break;

        case GameIds.EndingHero:
            AudioManager.Instance.Play(GameIds.SfxEndingHero);
            break;

        case GameIds.EndingCriminal:
            AudioManager.Instance.Play(GameIds.SfxEndingCriminal);
            break;

        case GameIds.EndingTrue:
            AudioManager.Instance.Play(GameIds.SfxEndingTrue);
            break;
    }
    }
}