using TMPro;
using UnityEngine;


public class Leaderboards : MonoBehaviour
{
    [SerializeField] private GameObject speedrunTimer;
    [SerializeField] private TextMeshProUGUI speedrunTimerText;
    private float timeElapsed = 0f;
    private bool speedrunTimerStarted = false;

    private void Start()
    {
        ToggleSpeedrunDisplay();
    }


    public void ToggleSpeedrunDisplay()
    {
        speedrunTimer.SetActive(PlayerPrefs.GetInt("ShowSpeedrunTimer", 0) == 1);
    }

    private void Update()
    {
        if (speedrunTimerStarted)
        {
            timeElapsed += Time.deltaTime;
            UpdateSpeedrunTimerText();
        }
    }


    private void UpdateSpeedrunTimerText()
    {
        int hours = Mathf.FloorToInt(timeElapsed / 3600);
        int minutes = Mathf.FloorToInt((timeElapsed % 3600) / 60);
        int seconds = Mathf.FloorToInt(timeElapsed % 60);

        if (hours > 99)
        {
            hours = 599;
        }

        if (hours > 0)
        {
            speedrunTimerText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }
        else
        {
            speedrunTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }


    public void StartSpeedRunTimer()
    {
        speedrunTimerStarted = true;
    }

    public void EndSpeedRunTimer()
    {
        speedrunTimerStarted = false;
        PostSpeedrunScore();
    }


    private void PostSpeedrunScore()
    {
        if (PlayerPrefs.GetInt("AutoPostScores", 1) == 1)
        {
            // TODO
        }
    }

    public void PostRoundScore(int waveNumber)
    {
        if (PlayerPrefs.GetInt("AutoPostScores", 1) == 1)
        {
            // TODO
        }
    }

}
