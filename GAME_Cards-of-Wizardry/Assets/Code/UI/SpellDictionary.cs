using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellDictionary : MonoBehaviour
{
    [SerializeField] private InputController inputController;
    [SerializeField] private SpellBook spellBook;
    [SerializeField] private GameObject spellBookPanel;
    [SerializeField] private GameObject spellBookEntryPrefab;
    [SerializeField] private Transform spellBookEntryContainer;
    [SerializeField] private AudioClip panelOpenAudio;
    [SerializeField] private AudioClip panelCloseAudio;
    [SerializeField] private AudioSource audioSource;

    [Header("Unlocked Textures")]
    [SerializeField] private Texture basicUnlockedCard;
    [SerializeField] private Texture flawlessUnlockedCard;
    [SerializeField] private Texture masterfulUnlockedCard;
    private bool isOpen = false;

    private void Update()
    {
        if (inputController.SpellBookPanelPressed && GameManager.Instance.UIPanelOpened == false)
        {
            OpenSpellCollectionPanel();
        }
        else if ((inputController.SpellBookPanelPressed || inputController.EscapePressed) && isOpen)
        {
            CloseSpellCollectionPanel();
        }
    }

    private void CreateSpellBookEntries()
    {
        // Create a new entry for each spell and their variations
        foreach (SpellBook.Spell spell in spellBook.spells)
        {
            // TODO: unlock image only if unlocked or mastered
            GameObject entryBasic = Instantiate(spellBookEntryPrefab, spellBookEntryContainer);
            GameObject entryFlawless = Instantiate(spellBookEntryPrefab, spellBookEntryContainer);
            GameObject entryMasterful = Instantiate(spellBookEntryPrefab, spellBookEntryContainer);

            RawImage basicCardImage = entryBasic.transform.Find("Card Image").GetComponent<RawImage>();
            RawImage flawlessCardImage = entryFlawless.transform.Find("Card Image").GetComponent<RawImage>();
            RawImage masterfulCardImage = entryMasterful.transform.Find("Card Image").GetComponent<RawImage>();

            TextMeshProUGUI basicManaCost = entryBasic.transform.Find("Mana Cost").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI flawlessManaCost = entryFlawless.transform.Find("Mana Cost").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI masterfulManaCost = entryMasterful.transform.Find("Mana Cost").GetComponent<TextMeshProUGUI>();

            TextMeshProUGUI basicCooldown = entryBasic.transform.Find("Delay Cooldown").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI flawlessCooldown = entryFlawless.transform.Find("Delay Cooldown").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI masterfulCooldown = entryMasterful.transform.Find("Delay Cooldown").GetComponent<TextMeshProUGUI>();

            if (PlayerPrefs.GetInt("BasicMastery_" + spell.name, 0) == 1)
            {
                basicCardImage.texture = spell.basicCardSprite;
                basicManaCost.text = spell.basicManaCost.ToString();
                basicCooldown.text = spell.basicCooldownDelay.ToString();
            }
            else
            {
                basicCardImage.texture = basicUnlockedCard;
                basicManaCost.text = "";
                basicCooldown.text = "";
            }

            if (PlayerPrefs.GetInt("FlawlessMastery_" + spell.name, 0) == 1)
            {
                flawlessCardImage.texture = spell.flawlessCardSprite;
                masterfulManaCost.text = spell.flawlessManaCost.ToString();
                masterfulCooldown.text = spell.flawlessCooldownDelay.ToString();
            }
            else
            {
                flawlessCardImage.texture = flawlessUnlockedCard;
                flawlessManaCost.text = "";
                flawlessCooldown.text = "";
            }

            if (PlayerPrefs.GetInt("MasterfulMastery_" + spell.name, 0) == 1)
            {
                masterfulCardImage.texture = spell.masterfulCardSprite;
                masterfulManaCost.text = spell.masterfulManaCost.ToString();
                masterfulCooldown.text = spell.masterfulCooldownDelay.ToString();
            }
            else
            {
                masterfulCardImage.texture = masterfulUnlockedCard;
                masterfulManaCost.text = "";
                masterfulCooldown.text = "";
            }

            basicCardImage.enabled = true;
            flawlessCardImage.enabled = true;
            masterfulCardImage.enabled = true;
        }
    }

    private void DeleteSpellBookEntries()
    {
        // Destroy the old entries after new ones are instantiated
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in spellBookEntryContainer)
        {
            children.Add(child.gameObject);
        }

        foreach (GameObject child in children)
        {
            Destroy(child);
        }
    }

    public void OpenSpellCollectionPanel()
    {
        GameManager.Instance.HideBasicUI();
        GameManager.Instance.UIPanelOpened = true;
        spellBookPanel.SetActive(true);
        CreateSpellBookEntries();

        audioSource.clip = panelOpenAudio;
        audioSource.Play();

        Time.timeScale = 0;

        isOpen = true;
    }

    public void CloseSpellCollectionPanel()
    {
        GameManager.Instance.ShowBasicUI();
        Time.timeScale = 1;
        spellBookPanel.SetActive(false);
        DeleteSpellBookEntries();

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
