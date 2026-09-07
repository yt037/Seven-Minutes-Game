// MainMenu.cs
// The house. Start and Quit are Card objects on the wall wired to the two
// methods below. The endings wall lists what this session has found.
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text endingsText;
    [SerializeField] private string endingsHeading = "ENDINGS FOUND";
    [SerializeField] private string endingsEmpty = "None yet.";

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
        FillEndingsWall();
    }

    private void FillEndingsWall()
    {
        if (endingsText == null) return;

        var sb = new StringBuilder();
        sb.Append(endingsHeading).Append("\n\n");

        GameManager gm = GameManager.Instance;
        if (gm == null || gm.State.endingsUnlocked.Count == 0)
        {
            sb.Append(endingsEmpty);
        }
        else
        {
            int i = 1;
            foreach (string id in gm.State.endingsUnlocked)
                sb.Append(i++).Append(". ").Append(Label(id)).Append('\n');
        }

        endingsText.text = sb.ToString().TrimEnd();
    }

    private static string Label(string ending)
    {
        switch (ending)
        {
            case GameIds.EndingFailure: return "Failure";
            case GameIds.EndingEscape: return "Escape";
            case GameIds.EndingHero: return "Hero";
            case GameIds.EndingCriminal: return "Criminal";
            case GameIds.EndingTrue: return "True";
            default: return ending;
        }
    }

    public void OnPlayPressed()
    {
        SceneManager.LoadScene(GameIds.SceneBank);
    }

    public void OnQuitPressed()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
