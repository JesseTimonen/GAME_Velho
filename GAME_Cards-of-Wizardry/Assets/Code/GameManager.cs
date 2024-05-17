using System.Collections;
using TMPro;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private SpellBook spellBook;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private LevelUpManager levelUpManager;
    [SerializeField] private Animator TitleScreenAnimator;
    [SerializeField] private GameObject UIStats;
    [SerializeField] private GameObject UIIcons;
    [SerializeField] private GameObject UICards;
    [SerializeField] private GameObject wis10CardUnlock;
    [SerializeField] private GameObject wis20CardUnlock;
    [SerializeField] private GameObject WindAudioObject;
    [SerializeField] private GameObject waveManager;
    [SerializeField] private GameObject survivalCurseDebuffIcon;
    [SerializeField] private TextMeshProUGUI survivalCurseText;
    [SerializeField] private GameObject fearDebuffIcon;

    public bool gameHasStarted = false;
    public bool UIPanelOpened = false;
    private float survivalModifier = 1f;
    public bool isFearActive = false;
    private Coroutine fearCoroutine;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        Settings();
    }


    public void StartGame()
    {
        TitleScreenAnimator.SetTrigger("StartGame");
        waveManager.SetActive(true);
        spellBook.Initialize();
        ShowBasicUI();
        gameHasStarted = true;
        WindAudioObject.SetActive(true);
    }


    private void Settings()
    {
        Application.targetFrameRate = 60;
    }


    public Transform GetPlayerTransform()
    {
        return playerTransform;
    }


    public PlayerController GetPlayerController()
    {
        return playerController;
    }


    public LevelUpManager GetLevelUpManager()
    {
        return levelUpManager;
    }


    public void HideBasicUI()
    {
        UIStats.SetActive(false);
        UIIcons.SetActive(false);
        UICards.SetActive(false);
    }


    public void ShowBasicUI()
    {
        UIStats.SetActive(true);
        UIIcons.SetActive(true);
        UICards.SetActive(true);

        if (playerController.GetWisdom() >= 10)
        {
            wis10CardUnlock.SetActive(true);
        }

        if (playerController.GetWisdom() >= 20)
        {
            wis20CardUnlock.SetActive(true);
        }
    }


    public void SetSurvivalModifier(int survivalWave = 0)
    {
        survivalModifier = 1f + 0.2f * survivalWave;

        if (survivalModifier > 1f)
        {
            survivalCurseDebuffIcon.SetActive(true);

            int buffAmount = Mathf.RoundToInt(survivalWave * 20f);
            survivalCurseText.text = "After the fall of the grand wizard you sense that the darkness grows quickly.\r\n\r\nEndless Curse Tier " + survivalWave + "\r\nEnemies have " + buffAmount + "% more health\r\nEnemies Deal " + buffAmount + "% more damage";
        }
    }


    public float GetSurvivalModifier()
    {
        return survivalModifier;
    }


    public void ToggleFear(bool isActive)
    {
        fearDebuffIcon.SetActive(isActive);
        isFearActive = isActive;

        if (isActive && fearCoroutine == null)
        {
            fearCoroutine = StartCoroutine(DealFearDamage());
        }
        else if (!isActive && fearCoroutine != null)
        {
            StopCoroutine(fearCoroutine);
            fearCoroutine = null;
        }
    }


    private IEnumerator DealFearDamage()
    {
        while (true)
        {
            playerController.TakeDamage(5);
            yield return new WaitForSeconds(1f);
        }
    }
}
