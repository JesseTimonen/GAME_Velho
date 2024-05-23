using TMPro;
using UnityEngine;

public class CharacterMenu : MonoBehaviour
{
    [SerializeField] private InputController inputController;
    [SerializeField] private LevelUpManager levelUpManager;
    [SerializeField] private GameObject characterPanel;
    [SerializeField] private AudioClip panelOpenAudio;
    [SerializeField] private AudioClip panelCloseAudio;
    [SerializeField] private AudioSource audioSource;
    private PlayerController playerController;
    private bool isOpen = false;

    [Header("Stat Texts")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private TextMeshProUGUI manaRechargeText;
    [SerializeField] private TextMeshProUGUI damageBoostText;
    [SerializeField] private TextMeshProUGUI damageReductionText;
    [SerializeField] private TextMeshProUGUI speedText;


    private void Start()
    {
        playerController = GameManager.Instance.GetPlayerController();
    }

    private void Update()
    {
        if (inputController.CharacterPanelPressed && GameManager.Instance.UIPanelOpened == false)
        {
            OpenCharacterPanel();
        }
        else if ((inputController.CharacterPanelPressed || inputController.EscapePressed) && isOpen)
        {
            CloseCharacterPanel();
        }
    }

    public void UpdateTexts()
    {
        // Health
        healthText.text = "Health: " + Mathf.Ceil(playerController.GetCurrentHealth()) + "/" + Mathf.Ceil(playerController.GetMaxHealth());
        if (playerController.GetTemporaryHealth() > 0)
        {
            healthText.text += " <color=#FFB400>(+" + Mathf.Ceil(playerController.GetTemporaryHealth()) + ")</color>";
        }

        // Mana
        manaText.text = "Mana:" + Mathf.Ceil(playerController.GetCurrentMana()) + "/" + Mathf.Ceil(playerController.GetMaxMana());
        if (playerController.GetTemporaryMana() > 0)
        {
            manaText.text += " <color=#FFB400>(+" + Mathf.Ceil(playerController.GetTemporaryMana()) + ")</color>";
        }

        // Mana Recharge
        manaRechargeText.text = "Mana Regeneration:" + playerController.GetCurrentManaRecharge().ToString("F2");

        // Damage Boost
        damageBoostText.text = "Damage Boost:" + Mathf.Round(playerController.GetDamageBoost() * 100f) + "%";
        float extraDamageBoost = Mathf.Round((playerController.GetShieldDamageBoost() - 1) * 100f) + Mathf.Round((playerController.GetEmpowerDamageBoost() - 1) * 100f);
        if (extraDamageBoost > 0 )
        {
            damageBoostText.text += " <color=#FFB400>(+" + extraDamageBoost + "%)</color>";
        }

        // Damage Reduction
        damageReductionText.text = "Damage Reduction: " + Mathf.Round(playerController.GetDamageReduction() * 100f) + "%";

        // Run Speed
        speedText.text = "Run Speed: " + playerController.GetRunSpeed().ToString("F2");
    }

    public void OpenCharacterPanel()
    {
        GameManager.Instance.HideBasicUI();
        GameManager.Instance.UIPanelOpened = true;

        levelUpManager.ResetPointAllocations();
        levelUpManager.UpdateStatUI();

        UpdateTexts();

        characterPanel.SetActive(true);

        audioSource.clip = panelOpenAudio;
        audioSource.Play();

        Time.timeScale = 0;

        isOpen = true;
    }

    public void CloseCharacterPanel()
    {
        GameManager.Instance.ShowBasicUI();
        Time.timeScale = 1;
        characterPanel.SetActive(false);


        audioSource.clip = panelCloseAudio;
        audioSource.Play();

        isOpen = false;

        Invoke(nameof(CloseUIPanelReference), 0.1f);
    }

    private void CloseUIPanelReference()
    {
        GameManager.Instance.UIPanelOpened = false;
    }
}
