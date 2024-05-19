using System.Collections.Generic;
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

            if (PlayerPrefs.GetInt("BasicMastery_" + spell.name, 0) == 1)
            {
                basicCardImage.texture = spell.basicCardSprite;
            }

            if (PlayerPrefs.GetInt("FlawlessMastery_" + spell.name, 0) == 1)
            {
                flawlessCardImage.texture = spell.flawlessCardSprite;
            }

            if (PlayerPrefs.GetInt("MasterfulMastery_" + spell.name, 0) == 1)
            {
                masterfulCardImage.texture = spell.masterfulCardSprite;
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

        Invoke("CloseUIPanelReference", 0.1f);
    }

    private void CloseUIPanelReference()
    {
        GameManager.Instance.UIPanelOpened = false;
    }
}