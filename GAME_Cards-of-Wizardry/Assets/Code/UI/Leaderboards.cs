using TMPro;
using UnityEngine;

public class Leaderboards : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputController inputController;
    [SerializeField] private GameObject leaderboardPanel;
    private bool isOpen = false;
    
    [Header("Audio Clips")]
    [SerializeField] private AudioClip panelOpenAudio;
    [SerializeField] private AudioClip panelCloseAudio;
    [SerializeField] private AudioSource audioSource;

    [Header("Speedrun Timer")]
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

        if (inputController.LeaderboardsPanelPressed && GameManager.Instance.UIPanelOpened == false)
        {
            OpenLeaderboardPanel();
        }
        else if ((inputController.LeaderboardsPanelPressed || inputController.EscapePressed) && isOpen)
        {
            CloseLeaderboardPanel();
        }
    }

    public void OpenLeaderboardPanel()
    {
        GameManager.Instance.HideBasicUI();
        GameManager.Instance.UIPanelOpened = true;
        leaderboardPanel.SetActive(true);

        audioSource.clip = panelOpenAudio;
        audioSource.Play();

        Time.timeScale = 0;

        isOpen = true;
    }

    public void CloseLeaderboardPanel()
    {
        GameManager.Instance.DisplayAdditionalHealthManaBars();
        GameManager.Instance.ShowBasicUI();
        Time.timeScale = 1;
        leaderboardPanel.SetActive(false);

        audioSource.clip = panelCloseAudio;
        audioSource.Play();

        isOpen = false;

        Invoke(nameof(CloseUIPanelReference), 0.1f);
    }

    private void CloseUIPanelReference()
    {
        GameManager.Instance.UIPanelOpened = false;
    }

    private void UpdateSpeedrunTimerText()
    {
        int hours = Mathf.FloorToInt(timeElapsed / 3600);
        int minutes = Mathf.FloorToInt((timeElapsed % 3600) / 60);
        int seconds = Mathf.FloorToInt(timeElapsed % 60);

        if (hours > 99)
        {
            speedrunTimerText.text = $"{hours}:{minutes:00}:{seconds:00}";
        }
        else if (hours > 0)
        {
            speedrunTimerText.text = $"{hours:00}:{minutes:00}:{seconds:00}";
        }
        else
        {
            speedrunTimerText.text = $"{minutes:00}:{seconds:00}";
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

    private bool ShouldPostScores()
    {
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("Username", "")))
        {
            return false;
        }

        if (PlayerPrefs.GetInt("AutoPostScores", 1) == 0)
        {
            return false;
        }

        return true;
    }

    private void PostSpeedrunScore()
    {
        if (!ShouldPostScores()) return;

        // TODO: Implement speedrun score posting logic using timeElapsed as score
    }

    public void PostRoundScore(int waveNumber)
    {
        if (!ShouldPostScores()) return;

        // TODO: Implement score posting logic using waveNumber as score
    }
}
