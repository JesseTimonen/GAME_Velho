using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] private InputController inputController;
    [SerializeField] private SpellMastery spellMastery;
    [SerializeField] private Leaderboards leaderboards;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private AudioClip panelOpenAudio;
    [SerializeField] private AudioClip panelCloseAudio;
    [SerializeField] private AudioSource audioSource;
    private bool isOpen = false;

    [Header("Audio")]
    [SerializeField] private MusicManager musicManager;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider backgroundVolumeSlider;
    [SerializeField] private Slider spellVolumeSlider;
    [SerializeField] private Slider UIVolumeSlider;
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI backgroundVolumeText;
    [SerializeField] private TextMeshProUGUI spellVolumeText;
    [SerializeField] private TextMeshProUGUI UIVolumeText;
    [SerializeField] private AudioSource spellFeedbackAudioSource;
    [SerializeField] private AudioSource UIFeedbackAudioSource;
    [SerializeField] private float audioFeedbackDelay = 0.25f;
    private bool canPlayFeedbackAudio = true;

    [Header("Accessability")]
    [SerializeField] private Toggle disableScreenShakeToggle;
    [SerializeField] private Toggle showHPBarToggle;
    [SerializeField] private Toggle showManaBarToggle;

    [Header("Leaderboards")]
    [SerializeField] private Toggle showSpeedrunTimer;
    [SerializeField] private Toggle autoPostLeaderboardToggle;
    [SerializeField] private GameObject leaderboardUsername;
    [SerializeField] private TMP_InputField leaderboardUsernameField;
    [SerializeField] private TextMeshProUGUI invalidUsernameError;
    [SerializeField] private GameObject speedrunTimer;

    [Header("Progression")]
    [SerializeField] private TextMeshProUGUI resetProgressionRequestButton;
    [SerializeField] private GameObject confirmResetProgressionButton;
    [SerializeField] private GameObject cancelResetProgressionButton;
    [SerializeField] private GameObject resetProgressionFeedback;


    private void Update()
    {
        if (inputController.OptionsPanelPressed && GameManager.Instance.UIPanelOpened == false)
        {
            OpenOptionsPanel();
        }
        else if ((inputController.OptionsPanelPressed || inputController.EscapePressed) && isOpen)
        {
            CloseOptionsPanel();
        }
    }


    public void OpenOptionsPanel()
    {
        GameManager.Instance.HideBasicUI();
        GameManager.Instance.UIPanelOpened = true;
        optionsPanel.SetActive(true);
        resetProgressionFeedback.SetActive(false);

        audioSource.clip = panelOpenAudio;
        audioSource.Play();

        musicVolumeSlider.value = Mathf.RoundToInt(PlayerPrefs.GetFloat("MusicVolume", 0));
        backgroundVolumeSlider.value = Mathf.RoundToInt(PlayerPrefs.GetFloat("BackgroundVolume", 0));
        spellVolumeSlider.value = Mathf.RoundToInt(PlayerPrefs.GetFloat("SpellVolume", 0));
        UIVolumeSlider.value = Mathf.RoundToInt(PlayerPrefs.GetFloat("UIVolume", 0));
        UpdateVolumeSliders();

        showSpeedrunTimer.isOn = PlayerPrefs.GetInt("ShowSpeedrunTimer", 0) == 1;
        autoPostLeaderboardToggle.isOn = PlayerPrefs.GetInt("AutoPostScores", 0) == 1;
        leaderboardUsername.SetActive(PlayerPrefs.GetInt("AutoPostScores", 0) == 1);
        leaderboardUsernameField.text = PlayerPrefs.GetString("Username", "");
        invalidUsernameError.text = "";

        disableScreenShakeToggle.isOn = PlayerPrefs.GetInt("DisableScreenShake", 0) == 1;
        showHPBarToggle.isOn = PlayerPrefs.GetInt("ShowHPBar", 1) == 1;
        showManaBarToggle.isOn = PlayerPrefs.GetInt("ShowManaBar", 1) == 1;

        Time.timeScale = 0;

        isOpen = true;
    }

    public void CloseOptionsPanel()
    {
        if (!AreSettingsValid())
        {
            return;
        }

        SaveSettings();

        GameManager.Instance.DisplayAdditionalHealthManaBars();
        GameManager.Instance.ShowBasicUI();
        Time.timeScale = 1;
        optionsPanel.SetActive(false);

        leaderboards.ToggleSpeedrunDisplay();

        audioSource.clip = panelCloseAudio;
        audioSource.Play();

        isOpen = false;

        Invoke(nameof(CloseUIPanelReference), 0.1f);
    }


    private bool AreSettingsValid()
    {
        if (leaderboardUsername.activeSelf && string.IsNullOrEmpty(leaderboardUsernameField.text))
        {
            invalidUsernameError.text = "Username can not be empty";
            return false;
        }

        return true;
    }


    private void SaveSettings()
    {
        PlayerPrefs.SetInt("ShowHPBar", showHPBarToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("ShowManaBar", showManaBarToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("DisableScreenShake", disableScreenShakeToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("ShowSpeedrunTimer", showSpeedrunTimer.isOn ? 1 : 0);
        PlayerPrefs.SetInt("AutoPostScores", autoPostLeaderboardToggle.isOn ? 1 : 0);
        PlayerPrefs.SetString("Username", leaderboardUsernameField.text);
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        PlayerPrefs.SetFloat("BackgroundVolume", backgroundVolumeSlider.value);
        PlayerPrefs.SetFloat("SpellVolume", spellVolumeSlider.value);
        PlayerPrefs.SetFloat("UIVolume", UIVolumeSlider.value);
        PlayerPrefs.Save();
    }


    private void CloseUIPanelReference()
    {
        GameManager.Instance.UIPanelOpened = false;
    }


    public void PostScoresAutomatically()
    {
        leaderboardUsername.SetActive(autoPostLeaderboardToggle.isOn);
    }


    public void UpdateVolumeSliders()
    {
        float volume = musicVolumeSlider.value + 80;
        musicVolumeText.text = volume.ToString() + "%";

        volume = backgroundVolumeSlider.value + 80;
        backgroundVolumeText.text = volume.ToString() + "%";

        volume = spellVolumeSlider.value + 80;
        spellVolumeText.text = volume.ToString() + "%";

        volume = UIVolumeSlider.value + 80;
        UIVolumeText.text = volume.ToString() + "%";

        musicManager.ModifyVolumeLevels(musicVolumeSlider.value, backgroundVolumeSlider.value, spellVolumeSlider.value, UIVolumeSlider.value);
    }

    public void PlaySpellAudioFeedback()
    {
        if (canPlayFeedbackAudio)
        {
            spellFeedbackAudioSource.Play();
            canPlayFeedbackAudio = false;
            StartCoroutine(AudioFeedbackCooldown());
        }
    }

    public void PlayUIAudioFeedback()
    {
        if (canPlayFeedbackAudio)
        {
            UIFeedbackAudioSource.Play();
            canPlayFeedbackAudio = false;
            StartCoroutine(AudioFeedbackCooldown());
        }
    }

    private IEnumerator AudioFeedbackCooldown()
    {
        float start = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup < start + audioFeedbackDelay)
        {
            yield return null;
        }
        canPlayFeedbackAudio = true;
    }

    public void ResetProgressionRequest()
    {
        resetProgressionRequestButton.GetComponent<HoverEffect>().enabled = false;
        confirmResetProgressionButton.SetActive(true);
        cancelResetProgressionButton.SetActive(true);
        resetProgressionFeedback.SetActive(false);
    }

    public void CancelResetProgression()
    {
        resetProgressionRequestButton.GetComponent<HoverEffect>().enabled = true;
        confirmResetProgressionButton.SetActive(false);
        cancelResetProgressionButton.SetActive(false);
    }

    public void ResetProgression()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("InitialSetupCompleted", 1);
        PlayerPrefs.Save();

        spellMastery.MassUpdateSpellCollection();

        resetProgressionRequestButton.GetComponent<HoverEffect>().enabled = true;
        confirmResetProgressionButton.SetActive(false);
        cancelResetProgressionButton.SetActive(false);
        resetProgressionFeedback.SetActive(true);
    }
}
