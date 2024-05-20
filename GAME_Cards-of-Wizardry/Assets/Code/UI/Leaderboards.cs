using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

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

    [Header("Leaderboard Display")]
    [SerializeField] private TextMeshProUGUI speedrunScoresText;
    [SerializeField] private TextMeshProUGUI survivalScoresText;

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

        if (inputController.LeaderboardsPanelPressed && !GameManager.Instance.UIPanelOpened)
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

        StartCoroutine(FetchSpeedrunScores());
        StartCoroutine(FetchSurvivalScores());

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

        if (!GameManager.Instance.hasPlayerDied)
        {
            PostSpeedrunScore();
        }
    }

    private bool ShouldPostScores()
    {
        if (string.IsNullOrEmpty(PlayerPrefs.GetString("Username", "")))
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
        StartCoroutine(PostScoreCoroutine("speedrun", timeElapsed));
    }

    public void PostRoundScore(int waveNumber)
    {
        if (!ShouldPostScores()) return;
        StartCoroutine(PostScoreCoroutine("survival", waveNumber));
    }

    private IEnumerator PostScoreCoroutine(string mode, float score)
    {
        string url = "https://c2t6xbah.c2.suncomet.fi/post-score.php";
        string username = PlayerPrefs.GetString("Username", "");
        string scoreString = score.ToString();
        string data = username + scoreString + mode;
        string secretKey = "REPLACE_ME";
        string hmac = ComputeHmacSha256(secretKey, data);

        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("score", scoreString);
        form.AddField("mode", mode);
        form.AddField("hmac", hmac);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();
        }
    }

    private string ComputeHmacSha256(string key, string data)
    {
        var keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
        var dataBytes = System.Text.Encoding.UTF8.GetBytes(data);
        using (var hmacSha256 = new System.Security.Cryptography.HMACSHA256(keyBytes))
        {
            var hashBytes = hmacSha256.ComputeHash(dataBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

    private IEnumerator FetchSpeedrunScores()
    {
        speedrunScoresText.text = "Loading scores...";

        string url = "https://c2t6xbah.c2.suncomet.fi/get-speedrun-scores.php";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                speedrunScoresText.text = "Connection to leaderboards failed.";
            }
            else if (!string.IsNullOrEmpty(www.downloadHandler.text))
            {
                ProcessSpeedrunScores(www.downloadHandler.text);
            }
            else
            {
                speedrunScoresText.text = "";
            }
        }
    }

    private IEnumerator FetchSurvivalScores()
    {
        survivalScoresText.text = "Loading scores...";

        string url = "https://c2t6xbah.c2.suncomet.fi/get-survival-scores.php";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                survivalScoresText.text = "Connection to leaderboards failed.";
            }
            else if (!string.IsNullOrEmpty(www.downloadHandler.text))
            {
                ProcessSurvivalScores(www.downloadHandler.text);
            }
            else
            {
                survivalScoresText.text = "";
            }
        }
    }

    private void ProcessSpeedrunScores(string json)
    {
        ScoreList scores = JsonUtility.FromJson<ScoreList>(json);

        speedrunScoresText.text = "";

        for (int i = 0; i < scores.scores.Count; i++)
        {
            string color = "";
            switch (i)
            {
                case 0:
                    color = "#D5A500";
                    break;
                case 1:
                    color = "#B7B7B7";
                    break;
                case 2:
                    color = "#A17419";
                    break;
            }

            if (!string.IsNullOrEmpty(color))
            {
                speedrunScoresText.text += $"<color={color}>{scores.scores[i].score} - {scores.scores[i].username}</color>\n";
            }
            else
            {
                speedrunScoresText.text += $"{scores.scores[i].score} - {scores.scores[i].username}\n";
            }
        }
    }

    private void ProcessSurvivalScores(string json)
    {
        ScoreList scores = JsonUtility.FromJson<ScoreList>(json);

        survivalScoresText.text = "";

        for (int i = 0; i < scores.scores.Count; i++)
        {
            string color = "";
            switch (i)
            {
                case 0:
                    color = "#D5A500";
                    break;
                case 1:
                    color = "#B7B7B7";
                    break;
                case 2:
                    color = "#A17419";
                    break;
            }

            if (!string.IsNullOrEmpty(color))
            {
                survivalScoresText.text += $"<color={color}>{scores.scores[i].score} - {scores.scores[i].username}</color>\n";
            }
            else
            {
                survivalScoresText.text += $"{scores.scores[i].score} - {scores.scores[i].username}\n";
            }
        }
    }

    [System.Serializable]
    public class Score
    {
        public string username;
        public string score;
    }

    [System.Serializable]
    public class ScoreList
    {
        public List<Score> scores;
    }
}
