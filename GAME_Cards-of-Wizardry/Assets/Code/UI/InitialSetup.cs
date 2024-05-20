using TMPro;
using UnityEngine;

public class InitialSetup : MonoBehaviour
{
    [SerializeField] private GameObject titleScreenPanel;
    [SerializeField] private GameObject initialSetupPanel;
    [SerializeField] private GameObject leaderboardsRequest;
    [SerializeField] private GameObject usernameRequest;
    [SerializeField] private TMP_InputField leaderboardUsernameField;
    [SerializeField] private TextMeshProUGUI usernameErrorText;

    private void Start()
    {
        if (PlayerPrefs.GetInt("InitialSetupCompleted", 0) == 0)
        {
            leaderboardsRequest.SetActive(true);
        }
        else
        {
            initialSetupPanel.SetActive(false);
            titleScreenPanel.SetActive(true);
        }
    }

    public void AcceptLeaderboards()
    {
        leaderboardsRequest.SetActive(false);
        usernameRequest.SetActive(true);
    }

    public void InsertUsername()
    {
        if (string.IsNullOrEmpty(leaderboardUsernameField.text))
        {
            usernameErrorText.text = "Username cannot be empty.";
            return;
        }

        PlayerPrefs.SetInt("InitialSetupCompleted", 1);
        PlayerPrefs.SetInt("AutoPostScores", 1);
        PlayerPrefs.SetString("Username", leaderboardUsernameField.text);
        PlayerPrefs.Save();
        initialSetupPanel.SetActive(false);
        titleScreenPanel.SetActive(true);
    }

    public void DeclineLeaderboards()
    {
        PlayerPrefs.SetInt("InitialSetupCompleted", 1);
        PlayerPrefs.SetInt("AutoPostScores", 0);
        PlayerPrefs.Save();
        initialSetupPanel.SetActive(false);
        titleScreenPanel.SetActive(true);
    }
}
