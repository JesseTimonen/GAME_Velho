using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Collections.Generic;


public class SpellCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private SpellBook spellBook;
    [SerializeField] private SpellMastery spellMastery;
    [SerializeField] private Texture rechargeCardTexture;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private GameObject spellActionPromptText;
    [SerializeField] private Animator outOfManaAnimator;
    [SerializeField] private GameObject discardPile;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private float discardDelayDuration = 5f;
    [SerializeField] private float showSpellPreviewDelay = 0.33f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip DrawCardAudio;
    [SerializeField] private AudioClip CancelDrawCardAudio;
    [SerializeField] private AudioClip noManaAudio;

    private GameObject castingAreaIndicatorInstance;
    private List<int> spellWeightedIndices = new List<int>();
    private int currentSpellIndex;
    private Vector3 startPosition;
    private Transform originalParent;
    private RawImage cardImage;
    private bool isDragging = false;
    private bool isRecharging = false;
    private int masteryLevelTier = 1;
    private bool spellCancelled = false;
    private bool spellDiscarded = false;
    private PointerEventData currentEventData;
    private float cooldownTimeRemaining = 0f;


    private void Awake()
    {
        cardImage = GetComponent<RawImage>();
        startPosition = transform.position;
        originalParent = transform.parent;

        PickRandomSpell();
    }


    private void OnEnable()
    {
        transform.position = startPosition;
        transform.SetParent(originalParent);
    }


    private void Update()
    {
        if (isDragging && castingAreaIndicatorInstance != null)
        {
            UpdateCastingAreaPosition();
        }

        if (isDragging)
        {
            if (Input.GetMouseButtonDown(1) || GameManager.Instance.UIPanelOpened)
            {
                spellCancelled = true;
                OnEndDrag(currentEventData);
            }
            else if (Input.GetMouseButtonDown(2))
            {
                spellDiscarded = true;
                OnEndDrag(currentEventData);
            }
        }

        if (isRecharging)
        {
            cooldownTimeRemaining -= Time.deltaTime;

            if (cooldownTimeRemaining <= 0)
            {
                FinishRecharge();
            }
            else
            {
                cooldownText.text = Mathf.Ceil(cooldownTimeRemaining).ToString();
            }
        }
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isRecharging) return;

        isDragging = true;
        transform.SetParent(transform.root);
        cardImage.raycastTarget = false;
        currentEventData = eventData;

        spellActionPromptText.SetActive(true);
        discardPile.SetActive(true);

        audioSource.clip = DrawCardAudio;
        audioSource.Play();

        // Activate spell preview only if the spell has one
        if ((masteryLevelTier == 1 && spellBook.spells[currentSpellIndex].basicSpellPreview != null) ||
           (masteryLevelTier == 2 && spellBook.spells[currentSpellIndex].flawlessSpellPreview != null) ||
           (masteryLevelTier == 3 && spellBook.spells[currentSpellIndex].masterfulSpellPreview != null))
        {
            StartCoroutine(ShowSpellPreview());
        }
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (isRecharging) return;

        if (castingAreaIndicatorInstance != null)
        {
            castingAreaIndicatorInstance.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
        }
        else
        {
            transform.position = Input.mousePosition;
        }
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        if (isRecharging) return;

        isDragging = false;
        transform.position = startPosition;
        transform.SetParent(originalParent);
        cardImage.enabled = true;
        spellActionPromptText.SetActive(false);
        discardPile.SetActive(false);
        cardImage.raycastTarget = true;

        if (castingAreaIndicatorInstance != null)
        {
            Destroy(castingAreaIndicatorInstance);
        }

        if (!spellCancelled)
        {
            // Check if the drop was on the trash bin
            if (spellDiscarded || eventData.pointerCurrentRaycast.gameObject == discardPile)
            {
                StartRechargeSpell(discardDelayDuration);
                audioSource.clip = CancelDrawCardAudio;
                audioSource.Play();
            }
            else
            {
                TriggerCardEffect(eventData.position);
            }
        }
        else {
            audioSource.clip = CancelDrawCardAudio;
            audioSource.Play();
        }

        eventData.pointerDrag = null;
        spellCancelled = false;
        spellDiscarded = false;
    }


    private void PickRandomSpell()
    {
        spellWeightedIndices.Clear();
        for (int i = 0; i < spellBook.spells.Length; i++)
        {
            if (spellBook.spells[i].isUnlocked)
            {
                for (int j = 0; j < spellBook.spells[i].amountInDeck; j++)
                {
                    spellWeightedIndices.Add(i);
                }
            }
        }

     
        currentSpellIndex = spellWeightedIndices[Random.Range(0, spellWeightedIndices.Count)];
        masteryLevelTier = spellMastery.GetSpellMasteryLevel(spellBook.spells[currentSpellIndex].name);

        if (masteryLevelTier == 1)
        {
            cardImage.texture = spellBook.spells[currentSpellIndex].basicCardSprite;
        }
        else if (masteryLevelTier == 2)
        {
            cardImage.texture = spellBook.spells[currentSpellIndex].flawlessCardSprite;
        }
        else
        {
            cardImage.texture = spellBook.spells[currentSpellIndex].masterfulCardSprite;
        }
    }


    private IEnumerator ShowSpellPreview()
    {
        yield return new WaitForSeconds(showSpellPreviewDelay);

        if (isDragging)
        {
            cardImage.enabled = false;

            if (masteryLevelTier == 1)
            {
                castingAreaIndicatorInstance = Instantiate(spellBook.spells[currentSpellIndex].basicSpellPreview, transform.position, Quaternion.identity, transform.parent);
            }
            else if (masteryLevelTier == 2)
            {
                castingAreaIndicatorInstance = Instantiate(spellBook.spells[currentSpellIndex].flawlessSpellPreview, transform.position, Quaternion.identity, transform.parent);
            }
            else
            {
                castingAreaIndicatorInstance = Instantiate(spellBook.spells[currentSpellIndex].masterfulSpellPreview, transform.position, Quaternion.identity, transform.parent);
            }

            castingAreaIndicatorInstance.transform.SetAsLastSibling();
            UpdateCastingAreaPosition();
        }
    }


    private void TriggerCardEffect(Vector3 screenPosition)
    {
        if (isRecharging) return;

        SpellBook.Spell spell = spellBook.spells[currentSpellIndex];

        if (playerController.HasMana(spell.manaCost))
        {
            playerController.UseMana(spell.manaCost);

            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, Camera.main.nearClipPlane));
            Vector3 spawnPosition = new Vector3(worldPosition.x, worldPosition.y, 0);

            playerAnimator.SetTrigger("Attack");

            if (masteryLevelTier == 1)
            {
                Instantiate(spell.basicSpellPrefab, spawnPosition, Quaternion.identity);
            }
            else if (masteryLevelTier == 2)
            {
                Instantiate(spell.flawlessSpellPrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                Instantiate(spell.masterfulSpellPrefab, spawnPosition, Quaternion.identity);
            }

            castingAreaIndicatorInstance = null;
            spellMastery.IncreaseSpellUsage(spell.name);
            StartRechargeSpell();
        }
        else
        {
            outOfManaAnimator.SetTrigger("Show");
            audioSource.clip = noManaAudio;
            audioSource.Play();
        }
    }


    public void StartRechargeSpell(float cooldownDuration = -1f)
    {
        if (cooldownDuration == -1f)
        {
            cooldownDuration = spellBook.spells[currentSpellIndex].usageDelay;
        }

        cooldownTimeRemaining = cooldownDuration;
        cardImage.raycastTarget = false;
        isRecharging = true;
        cardImage.color = new Color(255f, 255f, 255f, 200 / 255f);
        cooldownText.gameObject.SetActive(true);
        cardImage.texture = rechargeCardTexture;
    }


    private void FinishRecharge()
    {
        cardImage.color = Color.white;
        cooldownText.gameObject.SetActive(false);
        cardImage.raycastTarget = true;
        isRecharging = false;

        PickRandomSpell();
    }


    private void UpdateCastingAreaPosition()
    {
        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);
        castingAreaIndicatorInstance.transform.position = Camera.main.ScreenToWorldPoint(mousePos);
    }
}