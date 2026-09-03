using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] private float loopDuration = 420f;
    [SerializeField] private TextMeshProUGUI text;

    public float timer { get; private set; }
    [SerializeField] private bool running = true;

    private const int StartHour = 10;
    private const int EndHour = 17;

    private void Start()
    {
        timer = 0f;
        UpdateDisplay();
    }

    private void Update()
    {
        if (!running)
            return;

        timer += Time.deltaTime;

        if (timer >= loopDuration)
        {
            timer = loopDuration;
            UpdateDisplay();

            GameManager.Instance.EndLoop();
            return;
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        float timePercentage = timer / loopDuration;

        float currentGameHour = Mathf.Lerp(StartHour, EndHour, timePercentage);

        int totalMinutes = Mathf.FloorToInt(currentGameHour * 60f);

        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;

        string period = hours >= 12 ? "PM" : "AM";

        int displayHour = hours % 12;

        if (displayHour == 0)
            displayHour = 12;

        text.text = $"{displayHour:00}:{minutes:00} {period}";
    }

    public void StopTimer()
    {
        running = false;
    }
}