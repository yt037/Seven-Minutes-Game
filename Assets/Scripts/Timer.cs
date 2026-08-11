using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Timer : MonoBehaviour
{
    public float loopDuration = 420f;

    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private GameObject image;
    [SerializeField] private Player player;
    [SerializeField] private bool check;

    public float timer;

    private void Start()
    {
        timer = loopDuration;
        check = false;
    }

    private void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }

        // 0 at the beginning, 1 at the end
        float progress = 1f - (timer / loopDuration);

        // 120 game minutes = 2 game hours
        float gameMinutes = progress * 420f;

        // Start at 10:00 AM
        int totalMinutes = (10 * 60) + Mathf.FloorToInt(gameMinutes);

        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;

        string period = hours >= 12 ? "PM" : "AM";

        int displayHour = hours;

        if (displayHour > 12)
        {
            displayHour -= 12;
        }

        text.text = $"{displayHour:00}:{minutes:00} {period}";

        if (timer <= 0 && !check)
        {
            check = true;
            TimerEnded();
        }

        if (GameManager.Instance.HasKeycard)
        {
            image.SetActive(true);
        }
    }

    private void TimerEnded()
    {
        if (GameManager.Instance.GuardWarned)
        {
            Debug.Log("Robbery Stopped");
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}