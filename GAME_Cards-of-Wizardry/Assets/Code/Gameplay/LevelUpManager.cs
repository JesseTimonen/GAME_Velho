using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Data;


public class LevelUpManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpellBook spellBook;
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Slider experienceSlider;
    [SerializeField] private Image[] cardImages;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI levelUpTitle;
    [SerializeField] private TextMeshProUGUI experienceText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI availableStatPointsText;
    [SerializeField] private TextMeshProUGUI availableBasePointsText;
    [SerializeField] private TextMeshProUGUI strengthText;
    [SerializeField] private TextMeshProUGUI intelligenceText;
    [SerializeField] private TextMeshProUGUI wisdomText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI manaText;

    [Header("Buttons")]
    [SerializeField] private Button increaseStrengthButton;
    [SerializeField] private Button increaseIntelligenceButton;
    [SerializeField] private Button increaseWisdomButton;
    [SerializeField] private Button increaseHealthButton;
    [SerializeField] private Button increaseManaButton;

    [Header("Views")]
    [SerializeField] private GameObject cancelButton;
    [SerializeField] private GameObject confirmButton;
    [SerializeField] private GameObject skipButton;
    [SerializeField] private GameObject skipRewardView;
    [SerializeField] private GameObject rewardSelectionView;
    [SerializeField] private CharacterMenu characterMenu;

    [Header("Leveling Configuration")]
    [SerializeField] private List<int> expThresholds = new List<int>();

    private int currentExperience = 0;
    private int bufferedExperience = 0;
    private int level = 1;
    private bool levelUpInProgress = false;
    private int availableStatPoints = 0;
    private int availableBasePoints = 0;
    private string selectedCardRewardName;
    private int selectedCardRewardIndex;
    private System.Random random = new System.Random();


    private void Start()
    {
        UpdateEXPSlider();
    }

    public void AddExperience(int amount)
    {
        if (levelUpInProgress)
        {
            bufferedExperience += amount;
        }
        else
        {
            bufferedExperience = 0;
            currentExperience += amount;
            CheckLevelUp();
        }

        UpdateEXPSlider();
    }

    private void UpdateEXPSlider()
    {
        experienceText.text = currentExperience + "/" + expThresholds[level];
        experienceSlider.value = currentExperience;
        experienceSlider.maxValue = expThresholds[level];
    }

    private void CheckLevelUp()
    {
        while (level < expThresholds.Count && currentExperience >= expThresholds[level])
        {
            LevelUp();
            if (levelUpInProgress) { break; }
        }
    }

    private void LevelUp()
    {
        levelUpInProgress = true;
        currentExperience -= expThresholds[level];
        level++;

        availableStatPoints += 2;
        availableBasePoints += 1;

        increaseStrengthButton.interactable = true;
        increaseIntelligenceButton.interactable = true;
        increaseWisdomButton.interactable = true;
        increaseHealthButton.interactable = true;
        increaseManaButton.interactable = true;

        playerController.SetHealthFull();
        playerController.SetManaFull();

        OpenLevelUpPanel();
    }

    private void OpenLevelUpPanel()
    {
        levelUpPanel.SetActive(true);
        GameManager.Instance.HideBasicUI();
        GameManager.Instance.UIPanelOpened = true;

        levelText.text = "Lv." + level.ToString();
        levelUpTitle.text = "You have reached Level " + level.ToString() + "!";

        cancelButton.SetActive(false);
        confirmButton.SetActive(false);
        skipButton.SetActive(true);

        rewardSelectionView.SetActive(true);
        skipRewardView.SetActive(false);

        ShowLevelUpCardSelection();
        UpdateStatUI();

        Time.timeScale = 0;
    }

    public void CloseLevelUpPanel()
    {
        GameManager.Instance.UIPanelOpened = false;
        GameManager.Instance.ShowBasicUI();
        levelUpPanel.SetActive(false);
        Time.timeScale = 1;
        levelUpInProgress = false;

        if (!string.IsNullOrEmpty(selectedCardRewardName))
        {
            spellBook.AddSpell(selectedCardRewardName);
        }

        AddExperience(bufferedExperience);
    }

    private void ShowLevelUpCardSelection()
    {
        List<SpellBook.Spell> eligibleSpells = spellBook.spells.Where(spell => spell.minLevel <= level).ToList();

        // Shuffle the list and pick the first three
        eligibleSpells = eligibleSpells.OrderBy(x => random.Next()).Take(3).ToList();

        for (int i = 0; i < cardImages.Length; i++)
        {
            if (i < eligibleSpells.Count)
            {
                cardImages[i].gameObject.SetActive(true);
                cardImages[i].color = new Color(255, 255, 255, 1);
                cardImages[i].sprite = Sprite.Create((Texture2D)eligibleSpells[i].basicCardSprite, new Rect(0.0f, 0.0f, eligibleSpells[i].basicCardSprite.width, eligibleSpells[i].basicCardSprite.height), new Vector2(0.5f, 0.5f));
                cardImages[i].GetComponent<Button>().onClick.RemoveAllListeners();
                int index = i;
                cardImages[i].GetComponent<Button>().onClick.AddListener(() => SelectSpell(eligibleSpells[index], index));
            }
            else
            {
                cardImages[i].gameObject.SetActive(false);
            }
        }
    }

    private void SelectSpell(SpellBook.Spell spell, int selectedIndex)
    {
        selectedCardRewardName = spell.name;
        selectedCardRewardIndex = selectedIndex;

        for (int i = 0; i < cardImages.Length; i++)
        {
            if (i != selectedCardRewardIndex)
            {
                StartCoroutine(FadeOutCard(cardImages[i]));
            }
        }

        confirmButton.SetActive(true);
        cancelButton.SetActive(true);
        skipButton.SetActive(false);
    }

    private IEnumerator FadeOutCard(Image cardImage)
    {
        float duration = 0.5f;
        float currentTime = 0f;
        Color originalColor = cardImage.color;

        while (currentTime < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, currentTime / duration);
            cardImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            currentTime += Time.unscaledDeltaTime;
            yield return null;
        }

        cardImage.gameObject.SetActive(false);
    }

    private IEnumerator FadeInCard(Image cardImage)
    {
        float duration = 0.5f;
        float currentTime = 0f;
        Color originalColor = cardImage.color;
        cardImage.gameObject.SetActive(true);

        while (currentTime < duration)
        {
            float alpha = Mathf.Lerp(0f, 1f, currentTime / duration);
            cardImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            currentTime += Time.unscaledDeltaTime;
            yield return null;
        }

        cardImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
    }

    public void cancelSelectedReward()
    {
        selectedCardRewardName = "";

        for (int i = 0; i < cardImages.Length; i++)
        {
            if (i != selectedCardRewardIndex)
            {
                StartCoroutine(FadeInCard(cardImages[i]));
            }
        }

        confirmButton.SetActive(false);
        cancelButton.SetActive(false);
        skipButton.SetActive(true);
    }

    public void IncreaseStrength()
    {
        playerController.SetStrength(playerController.GetStrength() + 1);
        availableStatPoints--;
        UpdateStatUI();
    }

    public void IncreaseIntelligence()
    {
        playerController.SetIntelligence(playerController.GetIntelligence() + 1);
        availableStatPoints--;
        UpdateStatUI();
    }

    public void IncreaseWisdom()
    {
        playerController.SetWisdom(playerController.GetWisdom() + 1);
        availableStatPoints--;
        UpdateStatUI();
    }

    public void IncreaseMaxHealth()
    {
        playerController.AddMaxHealth(50);
        playerController.SetHealthFull();
        availableBasePoints--;
        UpdateStatUI();
    }

    public void IncreaseMaxMana()
    {
        playerController.AddMaxMana(10);
        playerController.SetManaFull();
        availableBasePoints--;
        UpdateStatUI();
    }

    private void UpdateStatUI()
    {
        availableStatPointsText.text = availableStatPoints + " Available!";
        availableBasePointsText.text = availableBasePoints +" Available!";
        strengthText.text = "Strength: " + playerController.GetStrength();
        intelligenceText.text = "Intelligence: " + playerController.GetIntelligence();
        wisdomText.text = "Wisdom: " + playerController.GetWisdom();
        healthText.text = "Max Health: " + playerController.GetMaxHealth();
        manaText.text = "Max Mana: " + playerController.GetMaxMana();

        if (availableStatPoints <= 0)
        {
            increaseStrengthButton.interactable = false;
            increaseIntelligenceButton.interactable = false;
            increaseWisdomButton.interactable = false;
            availableStatPointsText.text = "";
        }

        if (availableBasePoints <= 0)
        {
            increaseHealthButton.interactable = false;
            increaseManaButton.interactable = false;
            availableBasePointsText.text = "";
        }
    }

    public void closeSkipRewardView()
    {
        skipRewardView.SetActive(false);
        rewardSelectionView.SetActive(true);
    }

    public void openSkipRewardView()
    {
        rewardSelectionView.SetActive(false);
        skipRewardView.SetActive(true);
    }
}
