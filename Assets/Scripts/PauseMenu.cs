// PauseMenu.cs
// Escape pauses. Tab (clue log) does not; that lives in ClueLogUI.
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [SerializeField] private GameObject panel;

    private void Start()
    {
        IsPaused = false;
        if (panel != null) panel.SetActive(false);
    }

    private void Update()
    {
        if (!Keys.EscapePressed) return;

        if (!IsPaused && ClueLogUI.IsOpen)
        {
            if (ClueLogUI.Instance != null) ClueLogUI.Instance.Close();
            return;
        }

        if (IsPaused) Resume();
        else Pause();
    }

    private void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (panel != null) panel.SetActive(true);
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (panel != null) panel.SetActive(false);
    }

    public void OnMainMenuPressed()
    {
        Resume();
        if (GameManager.Instance != null) GameManager.Instance.GoToMenu();
        else SceneManager.LoadScene(GameIds.SceneMainMenu);
    }

    public void OnQuitPressed()
    {
        Time.timeScale = 1f;
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        IsPaused = false;
    }
}
