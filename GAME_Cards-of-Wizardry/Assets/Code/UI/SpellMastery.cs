using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using static SpellBook;

public class SpellMastery : MonoBehaviour
{
    [SerializeField] private InputController inputController;
    [SerializeField] private SpellBook spellBook;
    [SerializeField] private GameObject spellMasteryPanel;
    [SerializeField] private GameObject spellEntryPrefab;
    [SerializeField] private Transform spellEntryContainer;
    [SerializeField] private TextMeshProUGUI masteryNotificationText;
    [SerializeField] private Animator masteryNotificationAnimator;
    private Dictionary<string, int> spellUsage = new Dictionary<string, int>();
    [SerializeField] private AudioClip panelOpenAudio;
    [SerializeField] private AudioClip panelCloseAudio;
    [SerializeField] private AudioSource audioSource;
    private bool isOpen = false;

    private void Awake()
    {
        InitializeSpellUsage();
    }

    private void Update()
    {
        if (inputController.MasteryPanelPressed && GameManager.Instance.UIPanelOpened == false)
        {
            OpenMasteryPanel();
        }
        else if ((inputController.MasteryPanelPressed || inputController.EscapePressed) && isOpen)
        {
            CloseMasteryPanel();
        }
    }

    private void InitializeSpellUsage()
    {
        foreach (SpellBook.Spell spell in spellBook.spells)
        {
            spellUsage.Add(spell.name, 0);
        }
    }

    public void IncreaseSpellUsage(string spellName)
    {
        if (spellUsage.ContainsKey(spellName))
        {
            spellUsage[spellName]++;
            CheckAndUpdateMasteryLevel(spellName);
        }
        else
        {
            Debug.LogWarning("Spell name not found in mastery tracking: " + spellName);
        }
    }

    private void CheckAndUpdateMasteryLevel(string spellName)
    {
        int usageCount = spellUsage[spellName];
        SpellBook.Spell spell = System.Array.Find(spellBook.spells, s => s.name == spellName);
        if (string.IsNullOrEmpty(spell.name)) return;

        if (usageCount - spell.flawlessMasteryRequirement == spell.masterfulMasteryRequirement)
        {
            masteryNotificationText.text = "You have fully mastered " + spell.name + "!";
            masteryNotificationAnimator.SetTrigger("Show");
            PlayerPrefs.SetInt("MasterfulMastery_" + spellName, 1);
            PlayerPrefs.Save();
        }
        else if (usageCount == spell.flawlessMasteryRequirement)
        {
            masteryNotificationText.text = "You have learned flawless version of " + spell.name + "!";
            masteryNotificationAnimator.SetTrigger("Show");
            PlayerPrefs.SetInt("FlawlessMastery_" + spellName, 1);
            PlayerPrefs.Save();
        }
    }


    public void MassUpdateSpellCollection()
    {
        foreach (Spell spell in spellBook.spells)
        {
            if (spell.isUnlocked)
            {
                PlayerPrefs.SetInt("BasicMastery_" + spell.name, 1);

                // Check if the spell has reached flawless mastery
                if (spellUsage[spell.name] >= spell.flawlessMasteryRequirement)
                {
                    PlayerPrefs.SetInt("FlawlessMastery_" + spell.name, 1);
                }

                // Check if the spell has reached masterful mastery
                if (spellUsage[spell.name] >= spell.flawlessMasteryRequirement + spell.masterfulMasteryRequirement)
                {
                    PlayerPrefs.SetInt("MasterfulMastery_" + spell.name, 1);
                }
            }
        }

        PlayerPrefs.Save();
    }

    public int GetSpellMasteryLevel(string spellName)
    {
        int usageCount = spellUsage[spellName];
        SpellBook.Spell spell = System.Array.Find(spellBook.spells, s => s.name == spellName);

        if (usageCount - spell.flawlessMasteryRequirement >= spell.masterfulMasteryRequirement)
        {
            return 3;
        }
        else if (usageCount >= spell.flawlessMasteryRequirement)
        {
            return 2;
        }
        else
        {
            return 1;
        }
    }

    private void CreateSpellEntries()
    {
        // Create a new entry for each spell
        foreach (SpellBook.Spell spell in spellBook.spells)
        {

            if (spell.isUnlocked)
            {
                GameObject entry = Instantiate(spellEntryPrefab, spellEntryContainer);
                UpdateEntryUI(entry, spell);
            }
        }
    }

    private void DeleteSpellEntries()
    {
        // Destroy the old entries after new ones are instantiated
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in spellEntryContainer)
        {
            children.Add(child.gameObject);
        }

        foreach (GameObject child in children)
        {
            Destroy(child);
        }
    }

    private void UpdateEntryUI(GameObject entry, SpellBook.Spell spell)
    {
        entry.transform.Find("Spell Name").GetComponent<TextMeshProUGUI>().text = spell.name;
        TextMeshProUGUI rankText = entry.transform.Find("Spell Rank").GetComponent<TextMeshProUGUI>();
        Slider progressSlider = entry.transform.Find("Spell Mastery Slider").GetComponent<Slider>();
        TextMeshProUGUI progressText = entry.transform.Find("Spell Mastery Slider").Find("Mastery Progress Value").GetComponent<TextMeshProUGUI>();

        int currentUsage = spellUsage[spell.name];
        if (currentUsage - spell.flawlessMasteryRequirement >= spell.masterfulMasteryRequirement)
        {
            rankText.text = "Masterful";
            rankText.color = new Color(255, 185, 0, 1);
            progressSlider.maxValue = 1;
            progressSlider.value = 1;
            progressText.text = "Mastered";
        }
        else if (currentUsage >= spell.flawlessMasteryRequirement)
        {
            rankText.text = "Flawless";
            rankText.color = new Color(125, 0,255,1);
            progressSlider.maxValue = spell.masterfulMasteryRequirement;
            progressSlider.value = currentUsage - spell.flawlessMasteryRequirement;
            progressText.text = currentUsage - spell.flawlessMasteryRequirement + " / " + spell.masterfulMasteryRequirement;
        }
        else
        {
            rankText.text = "Basic";
            rankText.color = new Color(150, 150, 150, 1);
            progressSlider.maxValue = spell.flawlessMasteryRequirement;
            progressSlider.value = currentUsage;
            progressText.text = $"{currentUsage}/{spell.flawlessMasteryRequirement}";
        }
    }

    public void OpenMasteryPanel()
    {
        GameManager.Instance.HideBasicUI();
        GameManager.Instance.UIPanelOpened = true;
        spellMasteryPanel.SetActive(true);
        CreateSpellEntries();

        audioSource.clip = panelOpenAudio;
        audioSource.Play();

        Time.timeScale = 0;

        isOpen = true;
    }

    public void CloseMasteryPanel()
    {
        GameManager.Instance.ShowBasicUI();
        Time.timeScale = 1;
        DeleteSpellEntries();
        spellMasteryPanel.SetActive(false);

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
